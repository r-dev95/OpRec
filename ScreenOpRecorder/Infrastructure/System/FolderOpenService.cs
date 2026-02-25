using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Recording.Ports;

using Windows.Storage;
using Windows.System;

namespace ScreenOpRecorder.Infrastructure.System
{
    public sealed class FolderOpenService : IFolderOpenService
    {
        private readonly ILogger<FolderOpenService> _logger;

        public FolderOpenService(ILogger<FolderOpenService> logger)
        {
            _logger = logger;
        }

        public async Task OpenAsync(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open folder: {Path}", folderPath);
            }
        }
    }
}
