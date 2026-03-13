using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording.Audio
{
    public sealed class AudioMixer
    {
        private readonly ILogger<AudioMixer> _logger;

        public AudioMixer(ILogger<AudioMixer> logger)
        {
            _logger = logger;
        }

        public Task MixAsync(StorageFile micFile, StorageFile systemFile, StorageFile outputFile)
        {
            var disposables = new List<IDisposable>();
            try
            {
                var targetFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
                var micSample = PrepareInput(micFile.Path, targetFormat, disposables);
                var systemSample = PrepareInput(systemFile.Path, targetFormat, disposables);

                var mixer = new MixingSampleProvider(targetFormat)
                {
                    ReadFully = false
                };
                mixer.AddMixerInput(micSample);
                mixer.AddMixerInput(systemSample);

                var waveProvider = new SampleToWaveProvider16(mixer);
                MediaFoundationEncoder.EncodeToAac(waveProvider, outputFile.Path);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to mix audio.");
                throw;
            }
            finally
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            }
        }

        private static ISampleProvider PrepareInput(string path, WaveFormat targetFormat, List<IDisposable> disposables)
        {
            var reader = new MediaFoundationReader(path);
            disposables.Add(reader);

            ISampleProvider sample = reader.ToSampleProvider();
            if (sample.WaveFormat.SampleRate != targetFormat.SampleRate)
            {
                sample = new WdlResamplingSampleProvider(sample, targetFormat.SampleRate);
            }

            if (sample.WaveFormat.Channels != targetFormat.Channels)
            {
                sample = sample.WaveFormat.Channels switch
                {
                    1 when targetFormat.Channels == 2 => new MonoToStereoSampleProvider(sample),
                    2 when targetFormat.Channels == 1 => new StereoToMonoSampleProvider(sample),
                    _ => throw new InvalidOperationException("Unsupported channel conversion.")
                };
            }

            var volumeSample = new VolumeSampleProvider(sample)
            {
                Volume = 0.5f
            };
            return volumeSample;
        }

    }
}
