using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Ports;

using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class AudioCaptureService : IAudioCaptureService
    {
        private readonly ILogger<AudioCaptureService> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IFileManager _fileManager;

        private MediaCapture? _mediaCapture;
        private LowLagMediaRecording? _recording;

        private bool _isRecording;

        public AudioCaptureService(
            ILogger<AudioCaptureService> logger,
            IUserSettingsService settingsService,
            IFileManager fileManager)
        {
            _logger = logger;
            _settingsService = settingsService;
            _fileManager = fileManager;
        }

        public async Task<bool> StartAsync()
        {
            if (_isRecording)
            {
                return false;
            }

            try
            {
                await StartCaptureAsync();
                _isRecording = true;

                return true;
            }
            catch (Exception)
            {
                await StopAsync();
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRecording)
            {
                Creanup();
                _isRecording = false;
                return;
            }

            try
            {
                await _recording?.StopAsync();
            }
            finally
            {
                await _recording?.FinishAsync();
                Creanup();
                _isRecording = false;
            }
        }

        public void Dispose()
        {
            _ = StopAsync();
        }

        private async Task StartCaptureAsync()
        {
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio,
                AudioProcessing = AudioProcessing.Default,
                MediaCategory = MediaCategory.Media
            });

            _recording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Auto),
                _fileManager.FileList.AudioFilePath);

            await _recording.StartAsync();
        }

        private void Creanup()
        {
            _mediaCapture?.Dispose();
            _mediaCapture = null;

            _recording = null;
        }
    }
}
