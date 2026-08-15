using Microsoft.AspNetCore.Hosting;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Infrastructure.Services
{
    /// <summary>
    /// US-083. Stores images under wwwroot/uploads/{category}, served by the
    /// UseStaticFiles() middleware registered in Program.cs. Local disk is a deliberate
    /// choice for this single-instance, local-dev-only deployment (see ARCHITECTURE.md) —
    /// swap this implementation, not its callers, if cloud object storage is ever needed.
    /// </summary>
    public class LocalDiskFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalDiskFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(string? base64DataUri, string category, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(base64DataUri))
                return null;

            var (bytes, extension) = ParseDataUri(base64DataUri);

            var uploadsDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", category);
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            await File.WriteAllBytesAsync(Path.Combine(uploadsDir, fileName), bytes, cancellationToken);

            return $"/uploads/{category}/{fileName}";
        }

        public Task DeleteAsync(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith("/uploads/", StringComparison.Ordinal))
                return Task.CompletedTask;

            var filePath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        private static (byte[] Bytes, string Extension) ParseDataUri(string dataUri)
        {
            var commaIndex = dataUri.IndexOf(',');
            if (dataUri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex > 0)
            {
                var mimeType = dataUri[5..commaIndex].Split(';')[0];
                var extension = mimeType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".bin"
                };

                return (Convert.FromBase64String(dataUri[(commaIndex + 1)..]), extension);
            }

            // Defensive fallback — every current caller sends a proper data URI.
            return (Convert.FromBase64String(dataUri), ".png");
        }
    }
}
