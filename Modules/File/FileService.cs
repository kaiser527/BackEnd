using BackEnd.Modules.File.Dto;

namespace BackEnd.Modules.File
{
    public class FileService(IConfiguration configuration)
    {
        private readonly string _imageRoot = configuration["FileUpload:ImageUploadDir"]!;
        private readonly string _audioRoot = configuration["FileUpload:AudioUploadDir"]!;

        private readonly Dictionary<FileType, string[]> _allowedExtensions = new()
        {
            { FileType.Image, [".jpg", ".jpeg", ".png"] },
            { FileType.Audio, [".mp3", ".wav", ".ogg"] }
        };

        private readonly Dictionary<FileType, long> _maxFileSize = new()
        {
            { FileType.Image, 5 * 1024 * 1024 },
            { FileType.Audio, 15 * 1024 * 1024 }
        };

        private readonly Dictionary<UploadFolder, HashSet<string>> _protectedFiles = new()
        {
            {
                UploadFolder.User,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "admin.png",
                    "user.png"
                }
            }
        };

        public async Task<string> UploadAsync(IFormFile file, UploadFolder folder, FileType type)
        {
            Validate(file, type);

            var root = type == FileType.Image ? _imageRoot : _audioRoot;

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), root, folder.ToString().ToLower());

            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName);

            var fileName =
                $"{Path.GetFileNameWithoutExtension(file.FileName)}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";

            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            return fileName;
        }

        private void Validate(IFormFile file, FileType type)
        {
            if (file == null)
                throw new BadHttpRequestException("No file uploaded.");

            if (file.Length == 0)
                throw new BadHttpRequestException("File is empty.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions[type].Contains(extension))
                throw new BadHttpRequestException("Invalid file extension.");

            if (type == FileType.Image && !file.ContentType.StartsWith("image/"))
                throw new BadHttpRequestException("Invalid image.");

            if (type == FileType.Audio && !file.ContentType.StartsWith("audio/"))
                throw new BadHttpRequestException("Invalid audio.");

            if (file.Length > _maxFileSize[type])
                throw new BadHttpRequestException("File exceeds maximum size.");
        }

        public Task CleanupUnusedFilesAsync(FileType type, UploadFolder folder, IEnumerable<string> usedFiles)
        {
            var root = type == FileType.Image ? _imageRoot : _audioRoot;

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), root, folder.ToString().ToLower());

            if (!Directory.Exists(folderPath))
                return Task.CompletedTask;

            var used = usedFiles.Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var protectedFiles = _protectedFiles.TryGetValue(folder, out var files) ? files : [];

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileName = Path.GetFileName(file);

                if (protectedFiles.Contains(fileName))
                    continue;

                if (!used.Contains(fileName))
                {
                    System.IO.File.Delete(file);
                }
            }

            return Task.CompletedTask;
        }
    }
}