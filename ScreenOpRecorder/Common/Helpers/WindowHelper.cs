using System;
using System.Runtime.InteropServices;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.Graphics;
using Windows.Graphics.Capture;

using WinRT.Interop;

namespace ScreenOpRecorder.Common.Helpers
{
    internal static class WindowHelper
    {
        public static IntPtr GetHwnd(Window window)
        {
            return WindowNative.GetWindowHandle(window);
        }

        public static WindowId GetWindowId(Window window)
        {
            var hwnd = GetHwnd(window);
            return Win32Interop.GetWindowIdFromWindow(hwnd);
        }

        public static AppWindow GetAppWindow(Window window)
        {
            var windowId = GetWindowId(window);
            return AppWindow.GetFromWindowId(windowId);
        }

        public static DisplayArea GetDisplayArea(Window window, DisplayAreaFallback displayAreaFallback)
        {
            var windowId = GetWindowId(window);
            return DisplayArea.GetFromWindowId(windowId, displayAreaFallback);
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

        public static void MaximizeWindow(Window window)
        {
            var hwnd = GetHwnd(window);
            ShowWindow(hwnd, SW_MAXIMIZE);
        }

        public static void SetBorderAndTitleBar(Window window, bool hasBorder, bool hasTitleBar)
        {
            var appWindow = GetAppWindow(window);
            if (appWindow?.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(hasBorder, hasTitleBar);
            }
        }

        public static void Move(Window window, int x, int y)
        {
            var appWindow = GetAppWindow(window);
            appWindow?.Move(new PointInt32(x, y));
        }

        public static void Resize(Window window, int width, int height)
        {
            var appWindow = GetAppWindow(window);
            appWindow?.Resize(new SizeInt32(width, height));
        }
        public static void MoveAndResize(Window window, int x, int y, int width, int height)
        {
            var appWindow = GetAppWindow(window);
            appWindow?.MoveAndResize(new RectInt32(x, y, width, height));
        }

        public static double GetScaleFactor(Window window)
        {
            var hwnd = GetHwnd(window);
            uint dpi = GetDpiForWindow(hwnd);
            return dpi / 96.0; // 96 DPI = 100%
        }

        public static GraphicsCaptureItem CreateForMonitor(double x, double y, double width, double height)
        {
            var rect = new RECT()
            {
                left = (int)x,
                top = (int)y,
                right = (int)(x + width),
                bottom = (int)(y + height),

            };

            var hmon = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);
            var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            var ptr = interop.CreateForMonitor(new IntPtr(hmon), GraphicsCaptureItemGuid);
            var captureItem = GraphicsCaptureItem.FromAbi(ptr);
            return captureItem;
        }

        // --------------------------------------------------------------------
        // Win32 API
        // --------------------------------------------------------------------
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

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
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

        // --------------------------------------------------------------------
        // Windows.Graphics.Capture Interop IF
        // --------------------------------------------------------------------
        private const uint MONITOR_DEFAULTTONULL = 0;
        private const uint MONITOR_DEFAULTTOPRIMARY = 1;
        private const uint MONITOR_DEFAULTTONEAREST = 2;

        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private static Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        [ComImport]
        [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IGraphicsCaptureItemInterop
        {
            IntPtr CreateForWindow(
                [In] IntPtr window,
                [In] ref Guid iid);

            IntPtr CreateForMonitor(
                [In] IntPtr monitor,
                [In] ref Guid iid);
        }
    }
}
