using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Windows.System;

namespace OpRec.Infrastructure.Input
{
    internal static class InputHelper
    {
        public static readonly Dictionary<uint, string> ExcludeKeys = new()
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
        };

        public static readonly Dictionary<uint, string> ConvertKeys = new()
        {
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
        public static IntPtr SetHook(HookProc proc, int num)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;

            IntPtr hModule = GetModuleHandle(curModule.ModuleName);

            return SetWindowsHookEx(num, proc, hModule, 0);
        }

        // --------------------------------------------------------------------
        // Win32 API
        // --------------------------------------------------------------------
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;

        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12; // Alt

        public const int WH_MOUSE_LL = 14;
        public const int WM_LBUTTONDOWN = 0x0201;

        public const int SM_CXDOUBLECLK = 36;
        public const int SM_CYDOUBLECLK = 37;

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern int GetDoubleClickTime();

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);


        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

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
    }
}
