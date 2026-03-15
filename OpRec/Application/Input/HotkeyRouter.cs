using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpRec.Application.Settings.Ports;
using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Application.Input
{
    public sealed class HotkeyRouter : IHotkeyRouter, IDisposable
    {
        private readonly ILogger<HotkeyRouter> _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly List<HotkeyAction> _actions = new();
        private readonly Dictionary<HotkeyAction, Func<Task>> _handlers = new();
        private readonly Dictionary<HotkeyAction, bool> _handling = new();
        private readonly Dictionary<string, HotkeyAction> _bindings = new(StringComparer.OrdinalIgnoreCase);

        public HotkeyRouter(
            ILogger<HotkeyRouter> logger,
            IUserSettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
            _settingsService.SettingsChanged += OnSettingsChanged;
            ApplaySettings(_settingsService.Current);
        }

        public void Register(HotkeyAction action, Func<Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.ContainsKey(action))
            {
                _actions.Add(action);
            }

            _handlers[action] = handler;
            ApplaySettings(_settingsService.Current);
        }

        public async Task<bool> TryHandleAsync(string keyName)
        {
            var normalized = NormalizeHotkey(keyName);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (!_bindings.TryGetValue(normalized, out var action))
            {
                return false;
            }

            if (!_handlers.TryGetValue(action, out var handler))
            {
                return false;
            }

            if (_handling.TryGetValue(action, out var handling) && handling)
            {
                return true;
            }

            _handling[action] = true;
            try
            {
                await handler();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle hotkey action: {Action}", action);
                return true;
            }
            finally
            {
                _handling[action] = false;
            }
        }

        public void Dispose()
        {
            _settingsService.SettingsChanged -= OnSettingsChanged;
        }

        private void OnSettingsChanged(UserSettings settings)
        {
            ApplaySettings(settings);
        }

        private void ApplaySettings(UserSettings settings)
        {
            _bindings.Clear();

            foreach (var action in _actions)
            {
                var hotkey = GetHotkey(settings, action);
                var normalized = NormalizeHotkey(hotkey);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                if (_bindings.TryGetValue(normalized, out var existing) && existing != action)
                {
                    _logger.LogWarning("Duplicate hotkey '{Hotkey}' for actions {Existing} and {Action}.", normalized, existing, action);
                    continue;
                }

                _bindings[normalized] = action;
            }
        }

        private static string GetHotkey(UserSettings settings, HotkeyAction action)
        {
            return action switch
            {
                HotkeyAction.ToggleRecording => settings.ToggleRecordingHotkey,
                HotkeyAction.ToggleZoomAtCursor => settings.ToggleZoomHotkey,
                _ => ""
            };
        }

        private static string NormalizeHotkey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        }
    }
}
