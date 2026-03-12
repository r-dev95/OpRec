using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording.Audio
{
    public sealed class MicAudioCapture : IDisposable
    {
        private readonly ILogger<MicAudioCapture> _logger;

        private MediaCapture? _mediaCapture;
        private LowLagMediaRecording? _recording;
        private bool _isRecording;

        public MicAudioCapture(ILogger<MicAudioCapture> logger)
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start microphone capture.");
                await StopAsync();
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRecording)
            {
                Cleanup();
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
            Cleanup();
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

            var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);
            _recording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(profile, filePath);
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
