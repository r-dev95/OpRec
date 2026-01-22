using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ScreenOpRecorder.Features.Input
{
    internal static class InputHelper
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private const int SM_CXDOUBLECLK = 36;
        private const int SM_CYDOUBLECLK = 37;

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

        private static IntPtr SetHook(HookProc proc, int num)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;

            IntPtr hModule = GetModuleHandle(curModule.ModuleName);

            return SetWindowsHookEx(num, proc, hModule, 0);
        }

        public static IntPtr SetMouseHook(HookProc proc)
        {
            return SetHook(proc, WH_MOUSE_LL);
        }

        public static IntPtr SetKeyboardHook(HookProc proc)
        {
            return SetHook(proc, WH_KEYBOARD_LL);
        }

        public static bool UnHook(IntPtr hookId)
        {
            return UnhookWindowsHookEx(hookId);
        }

        public static IntPtr CallNextHook(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            return CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public static POINT GetCursorPos(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == WM_LBUTTONDOWN)
            {
                return Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam).pt;
            }

            return new POINT { x = dummyVal, y = dummyVal };
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
    }
}
