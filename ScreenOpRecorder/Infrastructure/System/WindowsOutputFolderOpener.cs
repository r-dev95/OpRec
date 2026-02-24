using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Recording.Ports;

using Windows.Storage;
using Windows.System;

namespace ScreenOpRecorder.Infrastructure.System
{
    public sealed class WindowsOutputFolderOpener : IOutputFolderOpener
    {
        private readonly ILogger<WindowsOutputFolderOpener> _logger;

        public WindowsOutputFolderOpener(ILogger<WindowsOutputFolderOpener> logger)
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
                _logger.LogWarning(ex, "Failed to open output folder: {Path}", folderPath);
            }
        }
    }
}
