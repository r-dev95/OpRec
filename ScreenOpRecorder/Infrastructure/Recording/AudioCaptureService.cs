using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class AudioCaptureService : IAudioCaptureService
    {
        private readonly ILogger<AudioCaptureService> _logger;

        private MediaCapture? _mediaCapture;
        private LowLagMediaRecording? _recording;

        private bool _isRecording;

        public AudioCaptureService(ILogger<AudioCaptureService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> StartAsync(StorageFile filePath)
        {
            if (_isRecording)
            {
                return false;
            }

            try
            {
                await StartCaptureAsync(filePath);
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
                Cleanup();
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
                Cleanup();
                _isRecording = false;
            }
        }

        public void Dispose()
        {
            _ = StopAsync();
        }

        private async Task StartCaptureAsync(StorageFile filePath)
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
                filePath);

            await _recording.StartAsync();
        }

        private void Cleanup()
        {
            _mediaCapture?.Dispose();
            _mediaCapture = null;

            _recording = null;
        }
    }
}
