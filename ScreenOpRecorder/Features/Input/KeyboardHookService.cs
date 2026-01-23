using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace ScreenOpRecorder.Features.Input
{
    public class KeyboardHookService : IKeyboardHookService
    {
        private readonly ILogger<KeyboardHookService> _logger;

        private IntPtr _hookId = IntPtr.Zero;
        private InputHelper.HookProc _hookProc;

        public event Action<string>? KeyDown;

        public KeyboardHookService(ILogger<KeyboardHookService> logger)
        {
            _logger = logger;
            _hookProc = HookCallback;
        }

        public void Start()
        {
            if (_hookId != IntPtr.Zero)
            {
                return;
            }

            _hookId = InputHelper.SetKeyboardHook(_hookProc);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                InputHelper.UnHook(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var keyName= InputHelper.GetKeyName(nCode, wParam, lParam);
            if (keyName != InputHelper.dummyVal.ToString())
            {
                KeyDown?.Invoke(keyName);
                _logger.LogDebug("Keyboard Event: {} at ({})", wParam, keyName);
            }

            return InputHelper.CallNextHook(_hookId, nCode, wParam, lParam);
        }
    }
}
