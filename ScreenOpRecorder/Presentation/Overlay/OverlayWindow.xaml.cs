using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Common.Helpers;
using ScreenOpRecorder.Presentation.Overlay.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Presentation.Overlay
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayWindow : Window
    {
        private readonly ILogger _logger;
        private readonly OverlayViewModel ViewModel;
        private readonly RipplePresenter _ripplePresenter;

        public OverlayWindow(ILogger<OverlayWindow> logger, OverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;
            _ripplePresenter = new RipplePresenter(OverlayCanvas);

            SetWindow();

            ViewModel.SetRecordingWindow += OnSetRecordingWindow;
            ViewModel.SetNotRecordingWindow += OnSetNotRecordingWindow;
            ViewModel.RippleRequested += OnRippleRequested;
            ViewModel.SetScaleFactor(WindowHelper.GetScaleFactor(this));
            ViewModel.Start();

            Closed += OnClosed;
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel?.SetRecordingWindow -= OnSetRecordingWindow;
            ViewModel?.SetNotRecordingWindow -= OnSetNotRecordingWindow;
            ViewModel?.RippleRequested -= OnRippleRequested;
            ViewModel?.Stop();
        }

        private void OnSetRecordingWindow()
        {
            WindowHelper.SetAlwaysOnTop(this, true);
            WindowHelper.SetClickThrough(this, true);
            MaskPath.Visibility = Visibility.Collapsed;
            ViewModel.EnterRecordingUiState(CaptureArea.StrokeThickness);
        }

        private void OnSetNotRecordingWindow()
        {
            WindowHelper.SetAlwaysOnTop(this, false);
            WindowHelper.SetClickThrough(this, false);
            MaskPath.Visibility = Visibility.Visible;
            ViewModel.ExitRecordingUiState();
        }

        private void OnRippleRequested(double x, double y, bool isDouble)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _ripplePresenter.Show(x, y, ViewModel.ClickHighlight, isDouble);
            });
        }

        private void SetWindow()
        {
            WindowHelper.SetBorderAndTitleBar(this, false, false);
            WindowHelper.MaximizeWindow(this);
            var scale = WindowHelper.GetScaleFactor(this);
            var physicalBounds = DpiHelper.ToPhysical(new Windows.Foundation.Size(Bounds.Width, Bounds.Height), scale);
            FullAreaRect.Rect = new(0, 0, physicalBounds.Width, physicalBounds.Height);
            ViewModel.InitializeWindowState(Bounds.Width, Bounds.Height);
        }
    }
}

