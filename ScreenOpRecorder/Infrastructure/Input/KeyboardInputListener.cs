using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Input.Ports;

using Windows.System;

namespace ScreenOpRecorder.Infrastructure.Input
{
    public class KeyboardInputListener : IKeyboardInputListener, IDisposable
    {
        private readonly ILogger<KeyboardInputListener> _logger;

        private IntPtr _hookId = IntPtr.Zero;
        private readonly InputHelper.HookProc _hookProc;
        private readonly object _gate = new();
        private int _startCount;

        public event Action<string>? KeyDown;

        public KeyboardInputListener(ILogger<KeyboardInputListener> logger)
        {
            _logger = logger;
            _hookProc = HookCallback;
        }

        public void Start()
        {
            lock (_gate)
            {
                _startCount++;
                if (_hookId != IntPtr.Zero)
                {
                    return;
                }

                _hookId = InputHelper.SetHook(_hookProc, InputHelper.WH_KEYBOARD_LL);
            }
        }

        public void Stop()
        {
            lock (_gate)
            {
                if (_startCount == 0)
                {
                    return;
                }

                _startCount--;
                if (_startCount > 0 || _hookId == IntPtr.Zero)
                {
                    return;
                }

                InputHelper.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        public void Dispose() => Stop();

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == InputHelper.WM_KEYDOWN || wParam == InputHelper.WM_SYSKEYDOWN))
            {
                var kbd = Marshal.PtrToStructure<InputHelper.KBDLLHOOKSTRUCT>(lParam);
                _logger.LogDebug("nCode[{}], wParam[{}], lParam[{}]", nCode, wParam, lParam);
                _logger.LogDebug("kbd.vkCode[{}]", kbd.vkCode);

                if (!InputHelper.ExcludeKeys.TryGetValue(kbd.vkCode, out _))
                {
                    if (!InputHelper.ConvertKeys.TryGetValue(kbd.vkCode, out var vkName))
                    {
                        vkName = ((VirtualKey)kbd.vkCode).ToString();
                    }
                    _logger.LogDebug("(VirtualKey)kbd.vkCode[{}], vkName[{}]", (VirtualKey)kbd.vkCode, vkName);

                    vkName = GetModifierCombinedKey(vkName);

                    KeyDown?.Invoke(vkName);

                    _logger.LogDebug("vkName[{}]", vkName);
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
                result.Add(InputHelper.ExcludeKeys[(uint)VirtualKey.Control]);
            }
            if ((InputHelper.GetKeyState(InputHelper.VK_SHIFT) & 0x8000) != 0)
            {
                result.Add(InputHelper.ExcludeKeys[(uint)VirtualKey.Shift]);
            }
            if ((InputHelper.GetKeyState(InputHelper.VK_MENU) & 0x8000) != 0)
            {
                result.Add(InputHelper.ExcludeKeys[(uint)VirtualKey.Menu]);
            }
            result.Add(vkName);

            return string.Join("+", result);
        }
    }
}
