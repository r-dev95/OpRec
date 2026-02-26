using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Ports;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class FileManager : IFileManager
    {
        public sealed class FilePathList
        {
            public StorageFile? FinalFilePath { get; set; }
            public StorageFile? VideoFilePath { get; set; }
            public StorageFile? AudioFilePath { get; set; }
        }

        private readonly ILogger<FileManager> _logger;
        private readonly IUserSettingsService _settingsService;

        public FilePathList FileList { get; private set; } = new();

        public FileManager(ILogger<FileManager> logger, IUserSettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            FileList.FinalFilePath = null;
            FileList.VideoFilePath = null;
            FileList.AudioFilePath = null;
        }

        public async Task<bool> SetupAsync()
        {
            try
            {
                var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                var dirPath = _settingsService.Current.OutputDirPath;
                Directory.CreateDirectory(dirPath);
                var localDir = await StorageFolder.GetFolderFromPathAsync(dirPath);
                FileList.FinalFilePath = await localDir.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                FileList.VideoFilePath = FileList.FinalFilePath;

                if (_settingsService.Current.EnableAudioCapture)
                {
                    var baseName = Path.GetFileNameWithoutExtension(FileList.FinalFilePath.Name);
                    FileList.VideoFilePath = await localDir.CreateFileAsync($"{baseName}.video.tmp.mp4", CreationCollisionOption.GenerateUniqueName);
                    FileList.AudioFilePath = await localDir.CreateFileAsync($"{baseName}.audio.tmp.m4a", CreationCollisionOption.GenerateUniqueName);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to setup file.");
                return false;
            }
        }

        public async void DeleteAsync()
        {
            await TryDeleteAsync(FileList.VideoFilePath);
            await TryDeleteAsync(FileList.AudioFilePath);
        }

        private async Task TryDeleteAsync(StorageFile? file)
        {
            try
            {
                await file?.DeleteAsync();
            }
            catch
            {
                _logger.LogWarning("failed to delete file. {}", file?.Path);
            }
        }
    }
}
