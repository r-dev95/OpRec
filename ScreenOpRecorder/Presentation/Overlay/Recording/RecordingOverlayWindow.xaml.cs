using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Presentation.Overlay.Recording.Helpers;

namespace ScreenOpRecorder.Presentation.Overlay.Recording
{
    public sealed partial class RecordingOverlayWindow : Window
    {
        private readonly ILogger _logger;
        private readonly RecordingOverlayViewModel ViewModel;
        private readonly RipplePresenter _ripplePresenter;

        public RecordingOverlayWindow(ILogger<RecordingOverlayWindow> logger, RecordingOverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;
            _ripplePresenter = new RipplePresenter(OverlayCanvas);

            SetWindow();

            Closed += OnClosed;
            ViewModel.SetRecordingUi += OnSetRecordingUi;
            ViewModel.UnSetRecordingUi += OnUnSetRecordingUi;
            ViewModel.ClickHighlightRequested += OnClickHighlightRequested;
            ViewModel.Start(WindowHelper.GetScaleFactor(this), Bounds.Width, Bounds.Height);
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel.SetRecordingUi -= OnSetRecordingUi;
            ViewModel.UnSetRecordingUi -= OnUnSetRecordingUi;
            ViewModel.ClickHighlightRequested -= OnClickHighlightRequested;
            ViewModel.Stop();
        }

        private void OnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, true);
        }
        private void OnUnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, false);
        }

        private void OnClickHighlightRequested(double x, double y, bool isDouble)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _ripplePresenter.Show(x, y, isDouble, ViewModel.ClickHighlight);
            });
        }

        private void SetWindow()
        {
            WindowHelper.SetBorderAndTitleBar(this, false, false);
            WindowHelper.SetClickThrough(this, true);
            WindowHelper.MaximizeWindow(this);
            WindowHelper.SetExcludeFromCapture(this, false);
        }
    }
}
