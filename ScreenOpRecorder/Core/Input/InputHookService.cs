using System;

using ScreenOpRecorder.Core.Input.Ports;

namespace ScreenOpRecorder.Core.Input
{
    public sealed class InputHookService : IInputHookService
    {
        private readonly IMouseInputListener _mouseInputListener;
        private readonly IKeyboardInputListener _keyboardInputListener;
        private bool _isStarted;

        public event Action<string>? KeyDown;
        public event Action<int, int, bool>? MouseClicked;

        public InputHookService(
            IMouseInputListener mouseInputListener,
            IKeyboardInputListener keyboardInputListener)
        {
            _mouseInputListener = mouseInputListener;
            _keyboardInputListener = keyboardInputListener;

            _mouseInputListener.MouseClicked += OnMouseClicked;
            _keyboardInputListener.KeyDown += OnKeyDown;
            Start();
        }

        public void Dispose()
        {
            _mouseInputListener.MouseClicked -= OnMouseClicked;
            _keyboardInputListener.KeyDown -= OnKeyDown;
            Stop();
        }

        private void Start()
        {
            if (_isStarted)
            {
                return;
            }

            _mouseInputListener.Start();
            _keyboardInputListener.Start();
            _isStarted = true;
        }

        private void Stop()
        {
            if (!_isStarted)
            {
                return;
            }

            _mouseInputListener.Stop();
            _keyboardInputListener.Stop();
            _isStarted = false;
        }

        private void OnKeyDown(string keyName)
        {
            KeyDown?.Invoke(keyName);
        }

        private void OnMouseClicked(int x, int y, bool isDouble)
        {
            MouseClicked?.Invoke(x, y, isDouble);
        }
    }
}
