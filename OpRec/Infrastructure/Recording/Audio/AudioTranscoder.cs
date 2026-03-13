using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NAudio.Wave;

using Windows.Storage;

namespace OpRec.Infrastructure.Recording.Audio
{
    public sealed class AudioTranscoder
    {
        private readonly ILogger<AudioTranscoder> _logger;

        public AudioTranscoder(ILogger<AudioTranscoder> logger)
        {
            _logger = logger;
        }

        public Task EncodeWavToM4aAsync(StorageFile inputWavFile, StorageFile outputM4aFile)
        {
            try
            {
                using var reader = new AudioFileReader(inputWavFile.Path);
                MediaFoundationEncoder.EncodeToAac(reader, outputM4aFile.Path);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to transcode audio to m4a.");
                throw;
            }
        }
    }
}
