using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Events.Ports;
using ScreenOpRecorder.Application.Recording.Events;
using ScreenOpRecorder.Application.Recording.Ports;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Domain.Settings.ValueObjects;
using ScreenOpRecorder.Domain.ValueObjects;
using ScreenOpRecorder.Infrastructure.Recording.Audio;
using ScreenOpRecorder.Infrastructure.Recording.Models;
using ScreenOpRecorder.Infrastructure.Recording.Video;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class RecordingService : IRecordingService
    {
        private readonly ILogger<RecordingService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly FileManager _fileManager;
        private readonly VideoCapture _videoCapture;
        private readonly AudioCapture _audioCapture;
        private readonly MediaFileMerger _mediaFileMerger;
        private readonly IEventBus _eventBus;

        private RecordingState _state = RecordingState.Waiting;
        private RecordingFiles? _recordingFiles;

        public string? LastOutputDirPath => throw new NotImplementedException();

        public RecordingService(
            ILogger<RecordingService> logger,
            IUserSettingsService settingsService,
            FileManager fileManager,
            VideoCapture videoCapture,
            AudioCapture audioCapture,
            MediaFileMerger mediaFileMerger,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _fileManager = fileManager;
            _videoCapture = videoCapture;
            _audioCapture = audioCapture;
            _mediaFileMerger = mediaFileMerger;
            _eventBus = eventBus;

            _videoCapture.RecordingStateChanged += OnRecordingStateChanged;
            _videoCapture.ZoomAreaChanged += OnZoomAreaChanged;
        }

        public bool TrySelectCaptureArea(ScreenRect captureArea)
        {
            return _videoCapture.TrySelectCaptureArea(captureArea);
        }

        public async Task<bool> StartAsync()
        {
            if (_state != RecordingState.Ready || !_videoCapture.HasSelectedCaptureArea)
            {
                return false;
            }

            try
            {
                _recordingFiles = await _fileManager.SetupAsync();
                if (_recordingFiles == null)
                {
                    return false;
                }

                var started = await _videoCapture.StartAsync(_recordingFiles.VideoFile);
                if (!started)
                {
                    await RollbackStartAsync("Failed to start display capture service.");
                    return false;
                }

                var shouldCaptureAudio = _settingsService.Current.AudioCaptureMode != AudioCaptureMode.Off;
                if (shouldCaptureAudio)
                {
                    var AudioFile = _recordingFiles.AudioFile;
                    if (AudioFile == null)
                    {
                        await RollbackStartAsync("Audio output file is not prepared.");
                        return false;
                    }

                    started = await _audioCapture.StartAsync(_recordingFiles);
                    if (!started)
                    {
                        await RollbackStartAsync("Failed to start audio capture service.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await RollbackStartAsync("Failed to start recording pipeline.", ex);
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_state != RecordingState.Recording)
            {
                _recordingFiles = null;
                return;
            }

            try
            {
                await _videoCapture.StopAsync();

                var shouldCaptureAudio = _settingsService.Current.AudioCaptureMode != AudioCaptureMode.Off;
                if (shouldCaptureAudio)
                {
                    await _audioCapture.StopAsync();
                    if (_recordingFiles != null)
                    {
                        var mergeSucceeded = await _mediaFileMerger.MergeAsync(_recordingFiles);
                        await _fileManager.CleanupAfterMergeAsync(_recordingFiles, mergeSucceeded);
                    }
                }
            }
            catch (Exception ex)
            {
                await FailSafeCleanupAsync();
                _logger.LogWarning(ex, "Failed to stop recording pipeline. Cleanup completed.");
                throw;
            }
            finally
            {
                _recordingFiles = null;
            }
        }

        private async Task RollbackStartAsync(string message, Exception? ex = null)
        {
            await FailSafeCleanupAsync();

            if (ex == null)
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogWarning(ex, message);
            }
        }

        private async Task FailSafeCleanupAsync()
        {
            try
            {
                await _videoCapture.StopAsync();
            }
            catch
            {
            }

            try
            {
                await _audioCapture.StopAsync();
            }
            catch
            {
            }

            await _fileManager.CleanupRecordingFilesAsync(_recordingFiles);
        }

        private void OnRecordingStateChanged(RecordingState state)
        {
            _state = state;
        }

        private void OnZoomAreaChanged(ScreenRect zoomRect)
        {
            _eventBus.Publish(new ZoomAreaChangedEvent(zoomRect));
        }
    }
}


