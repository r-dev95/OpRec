using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Settings.Ports;
using OpRec.Infrastructure.Recording.Models;
using OpRec.Infrastructure.Settings;

using Windows.Media.Editing;
using Windows.Media.Transcoding;

namespace OpRec.Infrastructure.Recording
{
    public sealed class MediaFileMerger
    {
        private readonly ILogger<MediaFileMerger> _logger;
        private readonly IUserSettingsService _settingsService;

        public MediaFileMerger(ILogger<MediaFileMerger> logger, IUserSettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
        }

        public async Task<bool> MergeAsync(RecordingFiles files)
        {
            var finalOutputFile = files.FinalFile;
            var videoOutputFile = files.VideoFile;
            var audioOutputFile = files.AudioFile;

            if (finalOutputFile == null || videoOutputFile == null || audioOutputFile == null)
            {
                return false;
            }

            try
            {
                var composition = new MediaComposition();
                var videoClip = await MediaClip.CreateFromFileAsync(videoOutputFile);
                var audioTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioOutputFile);
                composition.Clips.Add(videoClip);
                composition.BackgroundAudioTracks.Add(audioTrack);

                var profile = MediaEncodingProfile.CreateMp4(VideoQualitySelector.FromSettings(_settingsService.Current));
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

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to merge video and audio. Keeping video-only output.");
                return false;
            }
        }

    }
}

