using System;

using Microsoft.Extensions.Logging;

namespace ScreenOpRecorder.Features.Input
{
    public class MouseHookService : IDisposable
    {
        private readonly ILogger<MouseHookService> _logger;

        private IntPtr _hookId = IntPtr.Zero;
        private InputHelper.HookProc _hookProc;

        private DateTime _lastClickTime;
        private InputHelper.POINT _lastClickPoint;

        private int _doubleClickTime;
        private int _doubleClickX;
        private int _doubleClickY;

        public event Action<int, int, bool>? MouseClicked;

        public MouseHookService(ILogger<MouseHookService> logger)
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

            _hookId = InputHelper.SetMouseHook(_hookProc);

            _doubleClickTime = InputHelper.GetDoubleClkTime();
            _doubleClickX = InputHelper.GetDoubleClkX();
            _doubleClickY = InputHelper.GetDoubleClkY();
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
            var pt = InputHelper.GetCursorPos(nCode, wParam, lParam);
            if (pt.x != InputHelper.dummyVal && pt.y != InputHelper.dummyVal)
            {
                DateTime now = DateTime.Now;
                bool isDouble = IsDoubleClick(pt, now);
                MouseClicked?.Invoke(pt.x, pt.y, isDouble);
                _logger.LogDebug("Mouse Event: {} at ({}, {})", wParam, pt.x, pt.y);

                _lastClickTime = now;
                _lastClickPoint = pt;
            }

            return InputHelper.CallNextHook(_hookId, nCode, wParam, lParam);
        }

        private bool IsDoubleClick(InputHelper.POINT current, DateTime now)
        {
            var timeDiff = (now - _lastClickTime).TotalMilliseconds;
            if (timeDiff > _doubleClickTime)
            {
                return false;
            }

            if (Math.Abs(current.x - _lastClickPoint.x) > _doubleClickX / 2)
            {
                return false;
            }

            if (Math.Abs(current.y - _lastClickPoint.y) > _doubleClickY / 2)
            {
                return false;
            }
            _logger.LogDebug("Double Click Detected.");
            return true;
        }
    }
}
