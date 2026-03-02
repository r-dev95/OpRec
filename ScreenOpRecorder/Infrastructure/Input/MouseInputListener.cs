using System;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using ScreenOpRecorder.Application.Input.Ports;

namespace ScreenOpRecorder.Infrastructure.Input
{
    public class MouseInputListener : IMouseInputListener, IDisposable
    {
        private readonly ILogger<MouseInputListener> _logger;

        private IntPtr _hookId = IntPtr.Zero;
        private readonly InputHelper.HookProc _hookProc;

        private DateTime _lastClickTime;
        private InputHelper.POINT _lastClickPoint;

        private int _doubleClickTime;
        private int _doubleClickX;
        private int _doubleClickY;

        public event Action<int, int, bool>? MouseClicked;

        public MouseInputListener(ILogger<MouseInputListener> logger)
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

            _hookId = InputHelper.SetHook(_hookProc, InputHelper.WH_MOUSE_LL);

            _doubleClickTime = InputHelper.GetDoubleClickTime();
            _doubleClickX = InputHelper.GetSystemMetrics(InputHelper.SM_CXDOUBLECLK);
            _doubleClickY = InputHelper.GetSystemMetrics(InputHelper.SM_CYDOUBLECLK);
        }

        public void Stop()
        {
            if (_hookId == IntPtr.Zero)
            {
                return;
            }

            InputHelper.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        public void Dispose() => Stop();

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == InputHelper.WM_LBUTTONDOWN)
            {
                var pt = Marshal.PtrToStructure<InputHelper.MSLLHOOKSTRUCT>(lParam).pt;
                var now = DateTime.Now;
                var isDouble = IsDoubleClick(pt, now);
                MouseClicked?.Invoke(pt.x, pt.y, isDouble);

                _logger.LogDebug("nCode[{}], wParam[{}], lParam[{}]", nCode, wParam, lParam);
                _logger.LogDebug("pt.x[{}], pt.y[{}]), isDouble[{}]", pt.x, pt.y, isDouble);
            }

            return InputHelper.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool IsDoubleClick(InputHelper.POINT current, DateTime now)
        {
            var isDoubleClick = true;

            if ((now - _lastClickTime).TotalMilliseconds > _doubleClickTime)
            {
                isDoubleClick = false;
            }

            if (Math.Abs(current.x - _lastClickPoint.x) > _doubleClickX / 2)
            {
                isDoubleClick = false;
            }

            if (Math.Abs(current.y - _lastClickPoint.y) > _doubleClickY / 2)
            {
                isDoubleClick = false;
            }

            _lastClickTime = now;
            _lastClickPoint = current;

            return isDoubleClick;
        }
    }
}
