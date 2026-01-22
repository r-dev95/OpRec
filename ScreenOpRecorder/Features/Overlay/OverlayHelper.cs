using System;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace ScreenOpRecorder.Features.Overlay
{
    internal static class OverlayHelper
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private const int LWA_ALPHA = 0x2;

        private const int SW_MAXIMIZE = 3;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        private static IntPtr GetHwnd(Window window)
        {
            return WindowNative.GetWindowHandle(window);
        }

        public static void SetAlwaysOnTop(Window window, bool enable)
        {
            var hwnd = GetHwnd(window);

            SetWindowPos(
                hwnd,
                enable ? HWND_TOPMOST : HWND_NOTOPMOST,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW
            );
        }

        public static void SetClickThrough(Window window, bool enable)
        {
            var hwnd = GetHwnd(window);

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (enable)
            {
                exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
            }
            else
            {
                exStyle &= ~WS_EX_TRANSPARENT;
            }

            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        public static void SetWindowOpacity(Window window, byte opacity)
        {
            var hwnd = GetHwnd(window);
            SetLayeredWindowAttributes(hwnd, 0, opacity, LWA_ALPHA);
        }

        public static void MaximizeWindow(Window window)
        {
            var hwnd = GetHwnd(window);
            ShowWindow(hwnd, SW_MAXIMIZE);
        }

        public static double GetScaleFactor(Window window)
        {
            var hwnd = GetHwnd(window);
            uint dpi = GetDpiForWindow(hwnd);
            return dpi / 96.0; // 96 DPI = 100%
        }
    }
}
