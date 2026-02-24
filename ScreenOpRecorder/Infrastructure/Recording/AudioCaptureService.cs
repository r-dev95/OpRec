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

        public async Task<bool> StartAsync(StorageFile outputFile)
        {
            if (_isRecording)
            {
                return true;
            }

            try
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
                    outputFile);

                await _recording.StartAsync();
                _isRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audio capture failed to start. Continue with video only.");
                await StopAsync();
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_recording != null)
            {
                try
                {
                    if (_isRecording)
                    {
                        await _recording.StopAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Audio stop failed.");
                }
                finally
                {
                    try
                    {
                        await _recording.FinishAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Audio finish failed.");
                    }
                    _recording = null;
                }
            }

            _mediaCapture?.Dispose();
            _mediaCapture = null;
            _isRecording = false;
        }

        public void Dispose()
        {
            _ = StopAsync();
        }
    }
}
