using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Infrastructure.Settings
{
    public class UserSettingsService : IUserSettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly ILogger<UserSettingsService> _logger;
        private readonly string _settingsPath;

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
            var outputPath = settings.OutputFolderPath;
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

            var keySeconds = settings.KeyDisplayDurationSeconds;
            if (keySeconds < UserSettingsConstraints.MinKeyDisplayDurationSeconds
                || keySeconds > UserSettingsConstraints.MaxKeyDisplayDurationSeconds)
            {
                keySeconds = UserSettingsConstraints.DefaultKeyDisplayDurationSeconds;
            }

            var zoom = settings.ZoomFactor;
            if (zoom < UserSettingsConstraints.MinZoomFactor
                || zoom > UserSettingsConstraints.MaxZoomFactor)
            {
                zoom = UserSettingsConstraints.DefaultZoomFactor;
            }

            var clickSize = settings.ClickHighlightSize;
            if (clickSize < UserSettingsConstraints.MinClickHighlightSize
                || clickSize > UserSettingsConstraints.MaxClickHighlightSize)
            {
                clickSize = UserSettingsConstraints.DefaultClickHighlightSize;
            }

            var hotkey = string.IsNullOrWhiteSpace(settings.ToggleRecordingHotkey)
                ? UserSettingsConstraints.DefaultHotkey
                : settings.ToggleRecordingHotkey.Trim();

            var color = string.IsNullOrWhiteSpace(settings.ClickHighlightColor)
                ? UserSettingsConstraints.DefaultClickHighlightColor
                : settings.ClickHighlightColor.Trim();

            return new UserSettings
            {
                OutputFolderPath = outputPath,
                RecordingFps = fps,
                QualityPreset = settings.QualityPreset,
                EnableAudioCapture = settings.EnableAudioCapture,
                EnableClickHighlight = settings.EnableClickHighlight,
                ClickHighlightColor = color,
                ClickHighlightSize = clickSize,
                EnableKeyDisplay = settings.EnableKeyDisplay,
                KeyDisplayPosition = settings.KeyDisplayPosition,
                KeyDisplayDurationSeconds = keySeconds,
                EnableMinimap = settings.EnableMinimap,
                ZoomFactor = zoom,
                ToggleRecordingHotkey = hotkey,
                OpenOutputFolderAfterRecording = settings.OpenOutputFolderAfterRecording
            };
        }
    }
}
