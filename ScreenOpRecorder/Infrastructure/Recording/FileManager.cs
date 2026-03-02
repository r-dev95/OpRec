using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Infrastructure.Recording.Models;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class FileManager
    {
        private readonly ILogger<FileManager> _logger;
        private readonly IUserSettingsService _settingsService;

        public FileManager(ILogger<FileManager> logger, IUserSettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
        }

        public async Task<RecordingFiles?> SetupAsync()
        {
            try
            {
                var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                var dirPath = _settingsService.Current.OutputDirPath;
                Directory.CreateDirectory(dirPath);
                var localDir = await StorageFolder.GetFolderFromPathAsync(dirPath);
                var finalFilePath = await localDir.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                var videoFilePath = finalFilePath;
                StorageFile? audioFilePath = null;

                if (_settingsService.Current.EnableAudioCapture)
                {
                    var baseName = Path.GetFileNameWithoutExtension(finalFilePath.Name);
                    videoFilePath = await localDir.CreateFileAsync($"{baseName}.video.tmp.mp4", CreationCollisionOption.GenerateUniqueName);
                    audioFilePath = await localDir.CreateFileAsync($"{baseName}.audio.tmp.m4a", CreationCollisionOption.GenerateUniqueName);
                }

                return new RecordingFiles
                {
                    FinalFilePath = finalFilePath,
                    VideoFilePath = videoFilePath,
                    AudioFilePath = audioFilePath
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to setup file.");
                return null;
            }
        }

        public async Task CleanupRecordingFilesAsync(RecordingFiles? files)
        {
            if (files == null)
            {
                return;
            }

            await TryDeleteFileAsync(files.VideoFilePath);

            if (files.AudioFilePath != null)
            {
                await TryDeleteFileAsync(files.AudioFilePath);
            }

            if (files.FinalFilePath.Path != files.VideoFilePath.Path)
            {
                await TryDeleteFileAsync(files.FinalFilePath);
            }

            files = null;
        }

        private static async Task TryDeleteFileAsync(StorageFile file)
        {
            try
            {
                await file.DeleteAsync();
            }
            catch
            {
            }
        }
    }
}
