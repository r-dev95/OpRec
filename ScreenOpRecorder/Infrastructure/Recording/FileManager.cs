using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Domain.Settings.ValueObjects;
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
                var dirPath = _settingsService.Current.OutputDirPath;
                var localDir = await StorageFolder.GetFolderFromPathAsync(dirPath);
                Directory.CreateDirectory(localDir.Path);

                var baseName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}";
                var fileName = $"{baseName}.mp4";
                var FinalFile = await localDir.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                var VideoFile = FinalFile;
                StorageFile? AudioFile = null;

                if (_settingsService.Current.AudioCaptureMode != AudioCaptureMode.Off)
                {
                    VideoFile = await localDir.CreateFileAsync($"{baseName}.video.tmp.mp4", CreationCollisionOption.GenerateUniqueName);
                    var audioExtension = "m4a";
                    AudioFile = await localDir.CreateFileAsync($"{baseName}.audio.tmp.{audioExtension}", CreationCollisionOption.GenerateUniqueName);
                }

                var files = new RecordingFiles
                {
                    FinalFile = FinalFile,
                    VideoFile = VideoFile,
                    AudioFile = AudioFile
                };

                if (_settingsService.Current.AudioCaptureMode is AudioCaptureMode.Mic or AudioCaptureMode.Both && AudioFile != null)
                {
                    files.MicTempFile = await localDir.CreateFileAsync(
                        $"{baseName}.audio.mic.tmp.wav",
                        CreationCollisionOption.GenerateUniqueName);
                    files.AudioTempFiles.Add(files.MicTempFile);
                }

                if (_settingsService.Current.AudioCaptureMode is AudioCaptureMode.System or AudioCaptureMode.Both && AudioFile != null)
                {
                    files.SystemTempFile = await localDir.CreateFileAsync(
                        $"{baseName}.audio.sys.tmp.wav",
                        CreationCollisionOption.GenerateUniqueName);
                    files.AudioTempFiles.Add(files.SystemTempFile);
                }

                return files;
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

            await TryDeleteFileAsync(files.VideoFile);

            if (files.AudioFile != null)
            {
                await TryDeleteFileAsync(files.AudioFile);
            }

            await CleanupTempAudioFilesAsync(files);

            if (files.FinalFile.Path != files.VideoFile.Path)
            {
                await TryDeleteFileAsync(files.FinalFile);
            }

            files = null;
        }

        public async Task CleanupAfterMergeAsync(RecordingFiles files, bool mergeSucceeded)
        {
            if (mergeSucceeded)
            {
                await TryDeleteFileAsync(files.VideoFile);
                if (files.AudioFile != null)
                {
                    await TryDeleteFileAsync(files.AudioFile);
                }
                await CleanupTempAudioFilesAsync(files);
                return;
            }

            if (files.VideoFile.Path != files.FinalFile.Path)
            {
                await files.VideoFile.CopyAndReplaceAsync(files.FinalFile);
            }

            if (files.AudioFile != null)
            {
                await TryDeleteFileAsync(files.AudioFile);
            }

            await TryDeleteFileAsync(files.VideoFile);
            await CleanupTempAudioFilesAsync(files);
        }

        public async Task CleanupTempAudioFilesAsync(RecordingFiles files)
        {
            if (files.AudioTempFiles.Count == 0)
            {
                return;
            }

            foreach (var tempFile in files.AudioTempFiles)
            {
                await TryDeleteFileAsync(tempFile);
            }

            files.AudioTempFiles.Clear();
            files.MicTempFile = null;
            files.SystemTempFile = null;
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
