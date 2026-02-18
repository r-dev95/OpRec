using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Windows.System;

namespace ScreenOpRecorder.Features.Input
{
    public class KeyboardHookService : IDisposable
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

            _hookId = InputHelper.SetHook(_hookProc, InputHelper.WH_KEYBOARD_LL);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                InputHelper.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kbd = Marshal.PtrToStructure<InputHelper.KBDLLHOOKSTRUCT>(lParam);
                if (!InputHelper.ExcludeKeys.TryGetValue(kbd.vkCode, out var vkName))
                {
                    if (!InputHelper.ConvertKeys.TryGetValue(kbd.vkCode, out vkName))
                    {
                        vkName = ((VirtualKey)kbd.vkCode).ToString();
                    }
                    _logger.LogDebug("kbd.vkCode: {}, {}, {}", kbd.vkCode, (VirtualKey)kbd.vkCode, vkName);

                    if (wParam == InputHelper.WM_KEYDOWN || wParam == InputHelper.WM_SYSKEYDOWN)
                    {
                        vkName = GetModifierCombinedKey(vkName);

                        KeyDown?.Invoke(vkName);
                        _logger.LogDebug("Keyboard Event: {} at ({})", wParam, vkName);
                    }
                }
            }

            return InputHelper.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static string GetModifierCombinedKey(string vkName)
        {
            // 最上位ビットが1なら「押されている」状態
            var result = new List<string>();
            if ((InputHelper.GetKeyState(InputHelper.VK_CONTROL) & 0x8000) != 0)
            {
                result.Add("Ctrl");
            }
            if ((InputHelper.GetKeyState(InputHelper.VK_SHIFT) & 0x8000) != 0)
            {
                result.Add("Shift");
            }
            if ((InputHelper.GetKeyState(InputHelper.VK_MENU) & 0x8000) != 0)
            {
                result.Add("Alt");
            }
            result.Add(vkName);

            return string.Join("+", result);
        }
    }
}
