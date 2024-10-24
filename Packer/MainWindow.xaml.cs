using System.IO;
using System.IO.Compression;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Packer.Helpers;
using Packer.Models;
using Path = System.IO.Path;

namespace Packer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DirectoryPathTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputDirectoryPathTextBox.Text = dialog.FileName;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = DirectoryPathTextBox.Text;
            string outputDirectoryPath = OutputDirectoryPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                MessageBox.Show("Укажите корректный путь к директории для хеширования.");
                return;
            }

            if (string.IsNullOrWhiteSpace(outputDirectoryPath) || !Directory.Exists(outputDirectoryPath))
            {
                MessageBox.Show("Укажите корректный путь к директории для сохранения архивов.");
                return;
            }

            try
            {
                List<GameFile> files = await GetFileHashesAsync(directoryPath);
                string outputFilePath = Path.Combine(directoryPath, "file_hashes.json");
                await WorkerJson.SaveToJsonFileAsync(files, outputFilePath);
                OutputTextBox.Text = $"Хеши файлов сохранены в {outputFilePath}";

                await ArchiveFilesAsync(directoryPath, outputDirectoryPath);
                OutputTextBox.Text += "\nАрхивирование завершено.";
            }
            catch (Exception ex)
            {
                OutputTextBox.Text += $"\nПроизошла ошибка: {ex.Message}";
            }
        }

        private async Task<List<GameFile>> GetFileHashesAsync(string directoryPath)
        {
            List<GameFile> fileList = new List<GameFile>();

            string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            List<Task> tasks = new List<Task>();

            foreach (var file in files)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        string relativeFilePath = Path.GetRelativePath(directoryPath, file);
                        string hash = Hasher.GetHash(file);

                        GameFile fileData = new ()
                        {
                            Name = fileInfo.Name,
                            Path = relativeFilePath,
                            Size = (int)(fileInfo.Length / (1024 * 1024)),
                            Hash = hash
                        };

                        lock (fileList)
                        {
                            fileList.Add(fileData);
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputTextBox.Text += $"\nНе удалось вычислить хеш для файла {file}: {ex.Message}";
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return fileList;
        }

        private async Task ArchiveFilesAsync(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, filePath);
                string fileNameWithExtensions = Path.GetFileName(filePath);
                string fileDirectory = Path.GetDirectoryName(relativePath);
                string destinationFolder = Path.Combine(destinationDirectory, fileDirectory);

                Directory.CreateDirectory(destinationFolder);

                string destinationZipFile = Path.Combine(destinationFolder, fileNameWithExtensions + ".zip");

                if (File.Exists(destinationZipFile))
                {
                    OutputTextBox.Text += $"\nFile {destinationZipFile} already exists. Skipping...";
                    continue;
                }

                string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempFolder);

                string tempFilePath = Path.Combine(tempFolder, Path.GetFileName(filePath));
                File.Copy(filePath, tempFilePath, true);

                await Task.Run(() =>
                {
                    ZipFile.CreateFromDirectory(tempFolder, destinationZipFile, CompressionLevel.Optimal, false);
                });

                Directory.Delete(tempFolder, true);
            }
        }
    }
}