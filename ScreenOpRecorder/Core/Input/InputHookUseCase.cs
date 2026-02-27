using System;

using ScreenOpRecorder.Core.Input.Ports;

namespace ScreenOpRecorder.Core.Input
{
    public sealed class InputHookUseCase : IInputHookUseCase
    {
        private readonly IMouseHookService _mouseHookService;
        private readonly IKeyboardHookService _keyboardHookService;
        private bool _isStarted;

        public event Action<string>? KeyDown;
        public event Action<int, int, bool>? MouseClicked;

        public InputHookUseCase(
            IMouseHookService mouseHookService,
            IKeyboardHookService keyboardHookService)
        {
            _mouseHookService = mouseHookService;
            _keyboardHookService = keyboardHookService;

            _mouseHookService.MouseClicked += OnMouseClicked;
            _keyboardHookService.KeyDown += OnKeyDown;
            Start();
        }

        public void Dispose()
        {
            _mouseHookService.MouseClicked -= OnMouseClicked;
            _keyboardHookService.KeyDown -= OnKeyDown;
            Stop();
        }

        private void Start()
        {
            if (_isStarted)
            {
                return;
            }

            _mouseHookService.Start();
            _keyboardHookService.Start();
            _isStarted = true;
        }

        private void Stop()
        {
            if (!_isStarted)
            {
                return;
            }

            _mouseHookService.Stop();
            _keyboardHookService.Stop();
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
