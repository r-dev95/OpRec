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
                var videosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                outputPath = videosPath;
            }

            int fps = settings.RecordingFps;
            if (Array.IndexOf(UserSettingsConstraints.FpsOptions, fps) < 0)
            {
                fps = UserSettingsConstraints.Fps30;
            }

            var micVolume = settings.MicVolume;
            if (double.IsNaN(micVolume)
                || micVolume < UserSettingsConstraints.MinAudioVolume
                || micVolume > UserSettingsConstraints.MaxAudioVolume)
            {
                micVolume = UserSettingsConstraints.DefaultMicVolume;
            }

            var systemVolume = settings.SystemVolume;
            if (double.IsNaN(systemVolume)
                || systemVolume < UserSettingsConstraints.MinAudioVolume
                || systemVolume > UserSettingsConstraints.MaxAudioVolume)
            {
                systemVolume = UserSettingsConstraints.DefaultSystemVolume;
            }

            var zoom = settings.ZoomFactor;
            if (zoom < UserSettingsConstraints.MinZoomFactor
                || zoom > UserSettingsConstraints.MaxZoomFactor)
            {
                zoom = UserSettingsConstraints.DefaultZoomFactor;
            }

            var zoomInterpolation = settings.ZoomInterpolationSpeed;
            if (double.IsNaN(zoomInterpolation)
                || zoomInterpolation < UserSettingsConstraints.MinZoomInterpolationSpeed
                || zoomInterpolation > UserSettingsConstraints.MaxZoomInterpolationSpeed)
            {
                zoomInterpolation = UserSettingsConstraints.DefaultZoomInterpolationSpeed;
            }

            var keySeconds = settings.KeyDisplayDurationSeconds;
            if (keySeconds < UserSettingsConstraints.MinKeyDisplayDurationSeconds
                || keySeconds > UserSettingsConstraints.MaxKeyDisplayDurationSeconds)
            {
                keySeconds = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;
            }

            var clickSize = settings.ClickHighlightSize;
            if (clickSize < UserSettingsConstraints.MinClickHighlightSize
                || clickSize > UserSettingsConstraints.MaxClickHighlightSize)
            {
                clickSize = UserSettingsConstraints.DefaultClickHighlightSize;
            }

            var audioMode = Enum.IsDefined(typeof(AudioCaptureMode), settings.AudioCaptureMode)
                ? settings.AudioCaptureMode
                : AudioCaptureMode.Off;

            var color = string.IsNullOrWhiteSpace(settings.ClickHighlightColor)
                ? UserSettingsConstraints.DefaultClickHighlightColor
                : settings.ClickHighlightColor.Trim();

            var hotkey = string.IsNullOrWhiteSpace(settings.ToggleRecordingHotkey)
                ? UserSettingsConstraints.DefaultRecordingHotkey
                : settings.ToggleRecordingHotkey.Trim();

            var zoomHotkey = string.IsNullOrWhiteSpace(settings.ToggleZoomHotkey)
                ? UserSettingsConstraints.DefaultZoomHotkey
                : settings.ToggleZoomHotkey.Trim();

            return new UserSettings
            {
                OutputDirPath = outputPath,
                OpenDirectoryAfterRecording = settings.OpenDirectoryAfterRecording,
                RecordingFps = fps,
                QualityPreset = settings.QualityPreset,
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
