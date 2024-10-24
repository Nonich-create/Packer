using System.IO;
using System.Text.Json;
using Packer.Models;

namespace Packer.Helpers
{
    public static class WorkerJson
    {
        public static void SaveToJsonFile(Dictionary<string, string> fileHashes, string outputFilePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(fileHashes, options);
            File.WriteAllText(outputFilePath, jsonString);
        }

        public static async Task SaveToJsonFileAsync(Dictionary<string, string> fileHashes, string outputFilePath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(fileHashes, options);

            await File.WriteAllTextAsync(outputFilePath, jsonString);
        }

        public static void SaveToJsonFile(List<GameFile> files, string outputFilePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(files, options);
            File.WriteAllText(outputFilePath, jsonString);
        }

        public static async Task SaveToJsonFileAsync(List<GameFile> files, string outputFilePath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(files, options);

            await File.WriteAllTextAsync(outputFilePath, jsonString);
        }
    }
}
