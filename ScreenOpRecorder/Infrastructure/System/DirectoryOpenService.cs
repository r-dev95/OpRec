using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.System.Interfaces;

using Windows.Storage;
using Windows.System;

namespace ScreenOpRecorder.Infrastructure.System
{
    public sealed class DirectoryOpenService : IDirectoryOpenService
    {
        private readonly ILogger<DirectoryOpenService> _logger;

        public DirectoryOpenService(ILogger<DirectoryOpenService> logger)
        {
            _logger = logger;
        }

        public async Task OpenAsync(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
            {
                return;
            }

            try
            {
                var dir = await StorageFolder.GetFolderFromPathAsync(dirPath);
                await Launcher.LaunchFolderAsync(dir);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open directory: {Path}", dirPath);
            }
        }
    }
}
