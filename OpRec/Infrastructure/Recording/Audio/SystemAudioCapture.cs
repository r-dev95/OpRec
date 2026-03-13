using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NAudio.CoreAudioApi;
using NAudio.Wave;

using Windows.Storage;

namespace OpRec.Infrastructure.Recording.Audio
{
    public sealed class SystemAudioCapture : IDisposable
    {
        private readonly ILogger<SystemAudioCapture> _logger;

        private WasapiLoopbackCapture? _capture;
        private WaveFileWriter? _writer;
        private WasapiOut? _silenceOut;
        private IWaveProvider? _silenceProvider;
        private bool _isRecording;

        public SystemAudioCapture(ILogger<SystemAudioCapture> logger)
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
                _capture = new WasapiLoopbackCapture();
                _writer = new WaveFileWriter(filePath.Path, _capture.WaveFormat);
                _capture.DataAvailable += OnDataAvailable;

                StartSilentPlayback(_capture.WaveFormat);

                _capture.StartRecording();
                _isRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                await StopAsync();
                _logger.LogWarning(ex, "Failed to start system audio capture.");
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRecording)
            {
                Cleanup();
                StopSilentPlayback();
                return;
            }

            try
            {
                _capture?.StopRecording();
            }
            finally
            {
                Cleanup();
                StopSilentPlayback();
                _isRecording = false;
            }
        }

        public void Dispose()
        {
            Cleanup();
            StopSilentPlayback();
        }



        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
            _writer?.Flush();
        }

        private void Cleanup()
        {
            _capture?.DataAvailable -= OnDataAvailable;
            _capture?.Dispose();
            _capture = null;

            _writer?.Dispose();
            _writer = null;
        }

        private void StartSilentPlayback(WaveFormat format)
        {
            try
            {
                _silenceProvider = new SilenceProvider(format);
                _silenceOut = new WasapiOut(AudioClientShareMode.Shared, true, 0);
                _silenceOut.Init(_silenceProvider);
                _silenceOut.Play();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start silent playback.");
            }
        }

        private void StopSilentPlayback()
        {
            try
            {
                _silenceOut?.Stop();
            }
            catch
            {
            }

            _silenceOut?.Dispose();
            _silenceOut = null;
            _silenceProvider = null;
        }
    }
}
