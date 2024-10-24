using System.IO;
using System.Security.Cryptography;

namespace Packer.Helpers
{
    public static class Hasher
    {
        public static string GetHash(string filePath)
        {
            using (var hasher = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = hasher.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static async Task<string> GetHashAsync(string filePath)
        {
            using (var hasher = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    using (var memoryStream = new MemoryStream())
                    {
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        }

                        byte[] hashBytes = hasher.ComputeHash(memoryStream.ToArray());
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }
    }
}
