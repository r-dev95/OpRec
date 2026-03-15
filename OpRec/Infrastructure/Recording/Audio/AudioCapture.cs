using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Events.Ports;
using OpRec.Application.Recording.Events;
using OpRec.Application.Settings.Ports;
using OpRec.Domain.Settings.ValueObjects;
using OpRec.Infrastructure.Recording.Models;

using Windows.Storage;

namespace OpRec.Infrastructure.Recording.Audio
{
    public sealed class AudioCapture : IDisposable
    {
        private readonly ILogger<AudioCapture> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly MicAudioCapture _micAudioCapture;
        private readonly SystemAudioCapture _systemAudioCapture;
        private readonly AudioMixer _audioMixer;
        private readonly AudioTranscoder _audioTranscoder;
        private readonly IEventBus _eventBus;

        private bool _isRecording;
        private AudioCaptureModeOptions _mode = AudioCaptureModeOptions.Off;
        private StorageFile? _outputFile;
        private StorageFile? _micTempFile;
        private StorageFile? _systemTempFile;

        public AudioCapture(
            ILogger<AudioCapture> logger,
            IUserSettingsService settingsService,
            MicAudioCapture micAudioCapture,
            SystemAudioCapture systemAudioCapture,
            AudioMixer audioMixer,
            AudioTranscoder audioTranscoder,
            IEventBus eventBus)
        {
            _logger = logger;
            _settingsService = settingsService;
            _micAudioCapture = micAudioCapture;
            _systemAudioCapture = systemAudioCapture;
            _audioMixer = audioMixer;
            _audioTranscoder = audioTranscoder;
            _eventBus = eventBus;
        }

        public async Task<bool> StartAsync(RecordingFiles files)
        {
            if (_isRecording)
            {
                return false;
            }

            var settings = _settingsService.Current;
            _mode = settings.AudioCaptureMode;
            if (_mode == AudioCaptureModeOptions.Off)
            {
                return true;
            }

            _outputFile = files.AudioFile;
            _micTempFile = files.MicTempFile;
            _systemTempFile = files.SystemTempFile;

            try
            {
                var started = await StartCaptureAsync();
                if (!started)
                {
                    await RollbackStartAsync("Failed to start audio capture.");
                    return false;
                }

                _isRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                await RollbackStartAsync("Failed to start audio capture.", ex);
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
                await StopCaptureAsync();
            }
            catch (Exception ex)
            {
                PublishFailure("Failed to stop audio capture.", ex);
                throw;
            }
            finally
            {
                Cleanup();
                _isRecording = false;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        private async Task<bool> StartCaptureAsync()
        {
            switch (_mode)
            {
                case AudioCaptureModeOptions.Mic:
                    if (_micTempFile == null)
                    {
                        return false;
                    }
                    return await _micAudioCapture.StartAsync(_micTempFile);
                case AudioCaptureModeOptions.System:
                    if (_systemTempFile == null)
                    {
                        return false;
                    }
                    return await _systemAudioCapture.StartAsync(_systemTempFile);
                case AudioCaptureModeOptions.Both:
                    if (_micTempFile == null || _systemTempFile == null)
                    {
                        return false;
                    }

                    var micStarted = await _micAudioCapture.StartAsync(_micTempFile);
                    if (!micStarted)
                    {
                        return false;
                    }

                    var systemStarted = await _systemAudioCapture.StartAsync(_systemTempFile);
                    if (!systemStarted)
                    {
                        await _micAudioCapture.StopAsync();
                        return false;
                    }

                    return true;
                default:
                    return false;
            }
        }

        private async Task StopCaptureAsync()
        {
            var micVolume = (float)_settingsService.Current.MicVolume;
            var systemVolume = (float)_settingsService.Current.SystemVolume;

            switch (_mode)
            {
                case AudioCaptureModeOptions.Mic:
                    await _micAudioCapture.StopAsync();
                    if (_micTempFile == null || _outputFile == null)
                    {
                        throw new InvalidOperationException("Microphone temp file is not prepared.");
                    }
                    await _audioTranscoder.EncodeWavToM4aAsync(_micTempFile, _outputFile, micVolume);
                    return;
                case AudioCaptureModeOptions.System:
                    await _systemAudioCapture.StopAsync();
                    if (_systemTempFile == null || _outputFile == null)
                    {
                        throw new InvalidOperationException("System audio temp file is not prepared.");
                    }
                    await _audioTranscoder.EncodeWavToM4aAsync(_systemTempFile, _outputFile, systemVolume);
                    return;
                case AudioCaptureModeOptions.Both:
                    await _micAudioCapture.StopAsync();
                    await _systemAudioCapture.StopAsync();

                    if (_micTempFile == null || _systemTempFile == null || _outputFile == null)
                    {
                        throw new InvalidOperationException("Audio temp files are not prepared.");
                    }

                    await _audioMixer.MixAsync(_micTempFile, _systemTempFile, _outputFile, micVolume, systemVolume);
                    return;
                default:
                    return;
            }
        }

        private async Task RollbackStartAsync(string message, Exception? ex = null)
        {
            try
            {
                await _micAudioCapture.StopAsync();
            }
            catch
            {
            }

            try
            {
                await _systemAudioCapture.StopAsync();
            }
            catch
            {
            }

            PublishFailure(message, ex);
        }

        private void PublishFailure(string message, Exception? ex)
        {
            if (ex == null)
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogWarning(ex, message);
            }

            _eventBus.Publish(new AudioCaptureFailedEvent(message, ex));
        }

        private void Cleanup()
        {
            _outputFile = null;
            _micTempFile = null;
            _systemTempFile = null;
            _mode = AudioCaptureModeOptions.Off;
        }

    }
}
