using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Settings.Ports;
using OpRec.Domain.Settings.Policies;
using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Infrastructure.Settings
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly ILogger<UserSettingsService> _logger;
        private readonly string _settingsPath;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public UserSettings Current { get; private set; }

        public event Action<UserSettings>? SettingsChanged;

        public UserSettingsService(ILogger<UserSettingsService> logger)
        {
            _logger = logger;

            var appPath = AppContext.BaseDirectory;
            Directory.CreateDirectory(appPath);

            _settingsPath = Path.Combine(appPath, "usersettings.json");

            Current = LoadOrCreate();
        }

        public async Task SaveAsync(UserSettings settings)
        {
            var normalized = Normalize(settings);
            var tempPath = _settingsPath + ".tmp";

            var json = JsonSerializer.Serialize(normalized, JsonOptions);
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, _settingsPath, overwrite: true);

            Current = normalized;
            SettingsChanged?.Invoke(Current);
        }

        private UserSettings LoadOrCreate()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    var created = Normalize(new UserSettings());
                    File.WriteAllText(_settingsPath, JsonSerializer.Serialize(created, JsonOptions));
                    return created;
                }

                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<UserSettings>(json);
                if (loaded == null)
                {
                    return Normalize(new UserSettings());
                }

                var normalized = Normalize(loaded);
                if (json != JsonSerializer.Serialize(normalized, JsonOptions))
                {
                    File.WriteAllText(_settingsPath, JsonSerializer.Serialize(normalized, JsonOptions));
                }
                return normalized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load usersettings.json. Using defaults.");
                return Normalize(new UserSettings());
            }
        }

        private static UserSettings Normalize(UserSettings settings)
        {
            var outputPath = settings.OutputDirPath;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = UserSettingsDefaults.OutputDirPath;
            }

            var fps = Enum.IsDefined(typeof(VideoFpsOptions), settings.VideoFps)
                ? settings.VideoFps
                : UserSettingsDefaults.VideoFps;

            var videoQuality = Enum.IsDefined(typeof(VideoQualityOptions), settings.VideoQuality)
                ? settings.VideoQuality
                : UserSettingsDefaults.VideoQuality;

            var micVolume = settings.MicVolume;
            if (double.IsNaN(micVolume)
                || micVolume < UserSettingsConstraints.MinAudioVolume
                || micVolume > UserSettingsConstraints.MaxAudioVolume)
            {
                micVolume = UserSettingsDefaults.MicVolume;
            }

            var systemVolume = settings.SystemVolume;
            if (double.IsNaN(systemVolume)
                || systemVolume < UserSettingsConstraints.MinAudioVolume
                || systemVolume > UserSettingsConstraints.MaxAudioVolume)
            {
                systemVolume = UserSettingsDefaults.SystemVolume;
            }

            var zoom = settings.ZoomFactor;
            if (zoom < UserSettingsConstraints.MinZoomFactor
                || zoom > UserSettingsConstraints.MaxZoomFactor)
            {
                zoom = UserSettingsDefaults.ZoomFactor;
            }

            var zoomInterpolation = settings.ZoomInterpolationSpeed;
            if (double.IsNaN(zoomInterpolation)
                || zoomInterpolation < UserSettingsConstraints.MinZoomInterpolationSpeed
                || zoomInterpolation > UserSettingsConstraints.MaxZoomInterpolationSpeed)
            {
                zoomInterpolation = UserSettingsDefaults.ZoomInterpolationSpeed;
            }

            var keySeconds = settings.KeyDisplayDurationSeconds;
            if (keySeconds < UserSettingsConstraints.MinKeyDisplayDurationSeconds
                || keySeconds > UserSettingsConstraints.MaxKeyDisplayDurationSeconds)
            {
                keySeconds = UserSettingsDefaults.KeyDisplayDurationSeconds;
            }

            var clickSize = settings.ClickHighlightSize;
            if (clickSize < UserSettingsConstraints.MinClickHighlightSize
                || clickSize > UserSettingsConstraints.MaxClickHighlightSize)
            {
                clickSize = UserSettingsDefaults.ClickHighlightSize;
            }

            var audioMode = Enum.IsDefined(typeof(AudioCaptureModeOptions), settings.AudioCaptureMode)
                ? settings.AudioCaptureMode
                : UserSettingsDefaults.AudioCaptureMode;

            var color = string.IsNullOrWhiteSpace(settings.ClickHighlightColor)
                ? UserSettingsDefaults.ClickHighlightColor
                : settings.ClickHighlightColor.Trim();

            var hotkey = string.IsNullOrWhiteSpace(settings.ToggleRecordingHotkey)
                ? UserSettingsDefaults.ToggleRecordingHotkey
                : settings.ToggleRecordingHotkey.Trim();

            var zoomHotkey = string.IsNullOrWhiteSpace(settings.ToggleZoomHotkey)
                ? UserSettingsDefaults.ToggleZoomHotkey
                : settings.ToggleZoomHotkey.Trim();

            return new UserSettings
            {
                OutputDirPath = outputPath,
                OpenDirectoryAfterRecording = settings.OpenDirectoryAfterRecording,
                VideoFps = fps,
                VideoQuality = videoQuality,
                AudioCaptureMode = audioMode,
                MicVolume = micVolume,
                SystemVolume = systemVolume,
                EnableDoubleClickZoom = settings.EnableDoubleClickZoom,
                ZoomFactor = zoom,
                ZoomInterpolationSpeed = zoomInterpolation,
                EnableClickHighlight = settings.EnableClickHighlight,
                ClickHighlightColor = color,
                ClickHighlightSize = clickSize,
                EnableKeyDisplay = settings.EnableKeyDisplay,
                KeyDisplayPosition = settings.KeyDisplayPosition,
                KeyDisplayDurationSeconds = keySeconds,
                EnableMinimap = settings.EnableMinimap,
                ToggleRecordingHotkey = hotkey,
                ToggleZoomHotkey = zoomHotkey,
            };
        }
    }
}
