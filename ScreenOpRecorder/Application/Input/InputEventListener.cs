using System;

using ScreenOpRecorder.Application.Input.Ports;

namespace ScreenOpRecorder.Application.Input
{
    public sealed class InputEventListener : IInputEventListener
    {
        private readonly IMouseInputListener _mouseInputListener;
        private readonly IKeyboardInputListener _keyboardInputListener;

        public event Action<string>? KeyDown;
        public event Action<int, int, bool>? MouseClicked;

        public InputEventListener(
            IMouseInputListener mouseInputListener,
            IKeyboardInputListener keyboardInputListener)
        {
            _mouseInputListener = mouseInputListener;
            _keyboardInputListener = keyboardInputListener;

            _mouseInputListener.MouseClicked += OnMouseClicked;
            _keyboardInputListener.KeyDown += OnKeyDown;
            _mouseInputListener.Start();
            _keyboardInputListener.Start();
        }

        public void Dispose()
        {
            _mouseInputListener.MouseClicked -= OnMouseClicked;
            _keyboardInputListener.KeyDown -= OnKeyDown;
            _mouseInputListener.Stop();
            _keyboardInputListener.Stop();
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
