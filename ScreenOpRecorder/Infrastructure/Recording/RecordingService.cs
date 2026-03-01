using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Events.Interfaces;
using ScreenOpRecorder.Application.Recording.Events;
using ScreenOpRecorder.Application.Recording.Interfaces;
using ScreenOpRecorder.Application.Settings.Interfaces;
using ScreenOpRecorder.Domain.ValueObjects;
using ScreenOpRecorder.Infrastructure.Recording.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class RecordingService : IRecordingService
    {
        private readonly ILogger<RecordingService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly FileManager _fileManager;
        private readonly DisplayCaptureService _displayCaptureService;
        private readonly AudioCaptureService _audioCaptureService;
        private readonly MediaFileMerger _mediaFileMerger;
        private readonly IEventBus _eventBus;

        private RecordingState _state = RecordingState.Waiting;
        private RecordingFiles? _recordingFiles;

        public string? LastOutputDirPath { get; private set; }

        public RecordingService(
            ILogger<RecordingService> logger,
            IUserSettingsService settingsService,
            FileManager fileManager,
            DisplayCaptureService displayCaptureService,
            AudioCaptureService audioCaptureService,
            MediaFileMerger mediaFileMerger,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _fileManager = fileManager;
            _displayCaptureService = displayCaptureService;
            _audioCaptureService = audioCaptureService;
            _mediaFileMerger = mediaFileMerger;
            _eventBus = eventBus;

            _displayCaptureService.RecordingStateChanged += OnRecordingStateChanged;
            _displayCaptureService.ZoomAreaChanged += OnZoomAreaChanged;
        }

        public bool TrySelectCaptureArea(ScreenRect captureArea)
        {
            return _displayCaptureService.TrySelectCaptureArea(captureArea);
        }

        public async Task<bool> StartAsync()
        {
            if (_state != RecordingState.Ready || !_displayCaptureService.HasSelectedCaptureArea)
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
                LastOutputDirPath = Path.GetDirectoryName(_recordingFiles.FinalFilePath.Path);

                var started = await _displayCaptureService.StartAsync(_recordingFiles.VideoFilePath);
                if (!started)
                {
                    await RollbackStartAsync("Failed to start display capture service.");
                    return false;
                }

                if (_settingsService.Current.EnableAudioCapture)
                {
                    var audioFilePath = _recordingFiles.AudioFilePath;
                    if (audioFilePath == null)
                    {
                        await RollbackStartAsync("Audio output file is not prepared.");
                        return false;
                    }

                    started = await _audioCaptureService.StartAsync(audioFilePath);
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
                await _displayCaptureService.StopAsync();

                if (_settingsService.Current.EnableAudioCapture)
                {
                    await _audioCaptureService.StopAsync();
                    if (_recordingFiles != null)
                    {
                        await _mediaFileMerger.MergeAsync(_recordingFiles);
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
                await _displayCaptureService.StopAsync();
            }
            catch
            {
            }

            try
            {
                await _audioCaptureService.StopAsync();
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


