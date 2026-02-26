using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Common.Events;
using ScreenOpRecorder.Core.Recording.Events;
using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Domain.ValueObjects;
using ScreenOpRecorder.Infrastructure.Recording.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class RecordingService : IRecordingService
    {
        private readonly ILogger<RecordingService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IFileManager _fileManager;
        private readonly IDisplayCaptureService _displayCaptureService;
        private readonly IAudioCaptureService _audioCaptureService;
        private readonly IMediaFileMerger _mediaFileMerger;
        private readonly IEventBus _eventBus;

        private RecordingState _state = RecordingState.Waiting;

        public string? LastOutputDirPath { get; private set; }

        public RecordingService(
            ILogger<RecordingService> logger,
            IUserSettingsService settingsService,
            IFileManager fileManager,
            IDisplayCaptureService displayCaptureService,
            IAudioCaptureService audioCaptureService,
            IMediaFileMerger mediaFileMerger,
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
                var setuped = await _fileManager.SetupAsync();
                if (!setuped)
                {
                    return false;
                }
                LastOutputDirPath = _fileManager.FileList.FinalFilePath!.Name;

                var started = await _displayCaptureService.StartAsync();
                if (!started)
                {
                    await RollbackStartAsync("Failed to start display capture service.");
                    return false;
                }

                if (_settingsService.Current.EnableAudioCapture)
                {
                    started = await _audioCaptureService.StartAsync();
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
                _fileManager.Reset();
                return;
            }

            try
            {
                await _displayCaptureService.StopAsync();

                if (_settingsService.Current.EnableAudioCapture)
                {
                    await _audioCaptureService.StopAsync();
                    await _mediaFileMerger.MergeAsync();
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
                _fileManager.Reset();
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

            _fileManager.Reset();
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

