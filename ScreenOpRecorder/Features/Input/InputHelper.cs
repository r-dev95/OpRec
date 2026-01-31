using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Windows.System;

namespace ScreenOpRecorder.Features.Input
{
    internal static class InputHelper
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // Alt

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private const int SM_CXDOUBLECLK = 36;
        private const int SM_CYDOUBLECLK = 37;

        private static readonly Dictionary<uint, string> SpecialKeys = new()
        {
            { (uint)VirtualKey.Control, "Ctrl" },
            { (uint)VirtualKey.RightControl, "Ctrl" },
            { (uint)VirtualKey.LeftControl, "Ctrl" },
            { (uint)VirtualKey.Shift, "Shift" },
            { (uint)VirtualKey.RightShift, "Shift" },
            { (uint)VirtualKey.LeftShift, "Shift" },
            { (uint)VirtualKey.Menu, "Alt" },
            { (uint)VirtualKey.RightMenu, "Alt" },
            { (uint)VirtualKey.LeftMenu, "Alt" },
            { (uint)VirtualKey.RightWindows, "Win" },
            { (uint)VirtualKey.LeftWindows, "Win" },
            { (uint)VirtualKey.Number0, "0" },
            { (uint)VirtualKey.Number1, "1" },
            { (uint)VirtualKey.Number2, "2" },
            { (uint)VirtualKey.Number3, "3" },
            { (uint)VirtualKey.Number4, "4" },
            { (uint)VirtualKey.Number5, "5" },
            { (uint)VirtualKey.Number6, "6" },
            { (uint)VirtualKey.Number7, "7" },
            { (uint)VirtualKey.Number8, "8" },
            { (uint)VirtualKey.Number9, "9" },
        };

        public const int dummyVal = -5000;

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern int GetDoubleClickTime();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
        }

        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static IntPtr SetHook(int num, HookProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;

            IntPtr hModule = GetModuleHandle(curModule.ModuleName);

            return SetWindowsHookEx(num, proc, hModule, 0);
        }

        public static IntPtr SetMouseHook(HookProc proc)
        {
            return SetHook(WH_MOUSE_LL, proc);
        }

        public static IntPtr SetKeyboardHook(HookProc proc)
        {
            return SetHook(WH_KEYBOARD_LL, proc);
        }

        public static bool UnHook(IntPtr hookId)
        {
            return UnhookWindowsHookEx(hookId);
        }

        public static IntPtr CallNextHook(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            return CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public static int GetDoubleClkTime()
        {
            return GetDoubleClickTime();
        }

        public static int GetDoubleClkX()
        {
            return GetSystemMetrics(SM_CXDOUBLECLK);
        }

        public static int GetDoubleClkY()
        {
            return GetSystemMetrics(SM_CYDOUBLECLK);
        }

        public static POINT GetCursorPos(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == WM_LBUTTONDOWN)
            {
                return Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam).pt;
            }

            return new POINT { x = dummyVal, y = dummyVal };
        }

        private static string GetModifierCombinedKey(string vkName)
        {
            // 最上位ビットが1なら「押されている」状態
            bool ctrl = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
            bool shift = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
            bool alt = (GetKeyState(VK_MENU) & 0x8000) != 0;

            var result = new List<string>();
            if (ctrl)
            {
                result.Add("Ctrl");
            }
            if (shift)
            {
                result.Add("Shift");
            }
            if (alt)
            {
                result.Add("Alt");
            }
            result.Add(vkName);

            return string.Join("+", result);
        }

        public static string GetKeyName(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if (SpecialKeys.TryGetValue(kbd.vkCode, out var vkName))
                {
                    return dummyVal.ToString();
                }
                else
                {
                    vkName = ((VirtualKey)kbd.vkCode).ToString();
                }

                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    vkName = GetModifierCombinedKey(vkName);
                    return vkName;
                }
            }

            return dummyVal.ToString();
        }
    }
}
