using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Models;

using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class MediaFileMerger : IMediaFileMerger
    {
        private readonly ILogger<MediaFileMerger> _logger;

        public MediaFileMerger(ILogger<MediaFileMerger> logger)
        {
            _logger = logger;
        }

        public async Task MergeIfNeededAsync(OutputArtifacts artifacts, QualityPreset qualityPreset, int recordingFps)
        {
            if (!artifacts.CaptureAudio)
            {
                artifacts.Reset();
                return;
            }

            var videoOutputFile = artifacts.VideoOutputFile;
            var audioOutputFile = artifacts.AudioOutputFile;
            var finalOutputFile = artifacts.FinalOutputFile;

            if (videoOutputFile == null || audioOutputFile == null || finalOutputFile == null)
            {
                artifacts.Reset();
                return;
            }

            try
            {
                var composition = new MediaComposition();
                var videoClip = await MediaClip.CreateFromFileAsync(videoOutputFile);
                var audioTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioOutputFile);
                composition.Clips.Add(videoClip);
                composition.BackgroundAudioTracks.Add(audioTrack);

                var profile = MediaEncodingProfile.CreateMp4(ToVideoQuality(qualityPreset));
                profile.Video.FrameRate.Numerator = (uint)recordingFps;
                profile.Video.FrameRate.Denominator = 1;

                var result = await composition.RenderToFileAsync(
                    finalOutputFile,
                    MediaTrimmingPreference.Precise,
                    profile);

                if (result != TranscodeFailureReason.None)
                {
                    throw new InvalidOperationException($"Audio merge failed: {result}");
                }

                await videoOutputFile.DeleteAsync();
                await audioOutputFile.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audio merge failed. Keeping video-only output.");
                if (videoOutputFile.Path != finalOutputFile.Path)
                {
                    await videoOutputFile.CopyAndReplaceAsync(finalOutputFile);
                }

                try
                {
                    await audioOutputFile.DeleteAsync();
                }
                catch
                {
                }
            }
            finally
            {
                artifacts.Reset();
            }
        }

        private static VideoEncodingQuality ToVideoQuality(QualityPreset preset)
        {
            return preset switch
            {
                QualityPreset.Low => VideoEncodingQuality.Wvga,
                QualityPreset.Medium => VideoEncodingQuality.HD720p,
                _ => VideoEncodingQuality.HD1080p
            };
        }
    }
}
