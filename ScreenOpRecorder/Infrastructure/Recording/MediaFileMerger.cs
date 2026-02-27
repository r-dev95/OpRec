using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Models;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Infrastructure.Recording.Models;

using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class MediaFileMerger : IMediaFileMerger
    {
        private readonly ILogger<MediaFileMerger> _logger;
        private readonly IUserSettingsService _settingsService;

        public MediaFileMerger(ILogger<MediaFileMerger> logger, IUserSettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
        }

        public async Task MergeAsync(RecordingFiles files)
        {
            var finalOutputFile = files.FinalFilePath;
            var videoOutputFile = files.VideoFilePath;
            var audioOutputFile = files.AudioFilePath;

            if (finalOutputFile == null || videoOutputFile == null || audioOutputFile == null)
            {
                return;
            }

            try
            {
                var composition = new MediaComposition();
                var videoClip = await MediaClip.CreateFromFileAsync(videoOutputFile);
                var audioTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioOutputFile);
                composition.Clips.Add(videoClip);
                composition.BackgroundAudioTracks.Add(audioTrack);

                var profile = MediaEncodingProfile.CreateMp4(ToVideoQuality(_settingsService.Current.QualityPreset));
                profile.Video.FrameRate.Numerator = (uint)_settingsService.Current.RecordingFps;
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
                _logger.LogWarning(ex, "Failed to merge video and audio. Keeping video-only output.");
            }
            finally
            {
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

