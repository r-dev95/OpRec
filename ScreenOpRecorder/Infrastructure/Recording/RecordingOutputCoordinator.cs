using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Models;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class RecordingOutputCoordinator : IRecordingOutputCoordinator
    {
        private readonly ILogger<RecordingOutputCoordinator> _logger;
        private readonly IAudioCaptureService _audioCaptureService;

        public string? LastOutputFolderPath { get; private set; }

        public RecordingOutputCoordinator(ILogger<RecordingOutputCoordinator> logger, IAudioCaptureService audioCaptureService)
        {
            _logger = logger;
            _audioCaptureService = audioCaptureService;
        }

        public async Task PrepareAsync(RecordingOutputArtifacts artifacts, UserSettings settings)
        {
            string outputFolderPath = settings.OutputFolderPath;
            Directory.CreateDirectory(outputFolderPath);
            var localFolder = await StorageFolder.GetFolderFromPathAsync(outputFolderPath);
            var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

            var finalOutputFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            LastOutputFolderPath = localFolder.Path;

            artifacts.Reset();
            artifacts.FinalOutputFile = finalOutputFile;
            artifacts.VideoOutputFile = finalOutputFile;

            if (!settings.EnableAudioCapture)
            {
                return;
            }

            var baseName = Path.GetFileNameWithoutExtension(finalOutputFile.Name);
            var videoOutputFile = await localFolder.CreateFileAsync($"{baseName}.video.tmp.mp4", CreationCollisionOption.GenerateUniqueName);
            var audioOutputFile = await localFolder.CreateFileAsync($"{baseName}.audio.tmp.m4a", CreationCollisionOption.GenerateUniqueName);

            var started = await _audioCaptureService.StartAsync(audioOutputFile);
            if (!started)
            {
                _logger.LogWarning("Audio capture could not be started. Continue with video-only output.");
                await videoOutputFile.DeleteAsync();
                await audioOutputFile.DeleteAsync();
                return;
            }

            artifacts.CaptureAudio = true;
            artifacts.VideoOutputFile = videoOutputFile;
            artifacts.AudioOutputFile = audioOutputFile;
        }
    }
}
