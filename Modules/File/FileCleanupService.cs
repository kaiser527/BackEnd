using BackEnd.Modules.File.Dto;
using BackEnd.Modules.User;

namespace BackEnd.Modules.File
{
    public class FileCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<FileCleanupService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<FileCleanupService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var fileService = scope.ServiceProvider.GetRequiredService<FileService>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                await fileService.CleanupUnusedFilesAsync(
                    FileType.Image,
                    UploadFolder.User,
                    await userService.GetUserImages());

                // Later
                // await fileService.CleanupUnusedFilesAsync(
                //     FileType.Audio,
                //     UploadFolder.Question,
                //     await questionService.GetQuestionAudios());

                _logger.LogInformation("Unused files cleaned.");

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}