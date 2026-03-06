using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Common.Helpers;

namespace ScreenOpRecorder.Presentation.Overlay.Guide
{
    public sealed partial class GuideOverlayWindow : Window
    {
        private readonly ILogger _logger;
        private readonly GuideOverlayViewModel ViewModel;

        public GuideOverlayWindow(ILogger<GuideOverlayWindow> logger, GuideOverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            Closed += OnClosed;
            SetWindow();

            ViewModel.SetRecordingUi += OnSetRecordingUi;
            ViewModel.UnSetRecordingUi += OnUnSetRecordingUi;
            ViewModel.Start(WindowHelper.GetScaleFactor(this));
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel.SetRecordingUi -= OnSetRecordingUi;
            ViewModel.UnSetRecordingUi -= OnUnSetRecordingUi;
            ViewModel.Stop();
        }

        private void OnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, true);
            WindowHelper.SetClickThrough(this, true);
            MaskPath.Visibility = Visibility.Collapsed;
        }

        private void OnUnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, false);
            WindowHelper.SetClickThrough(this, false);
            MaskPath.Visibility = Visibility.Visible;
        }

        private void SetWindow()
        {
            WindowHelper.SetBorderAndTitleBar(this, false, false);
            WindowHelper.SetClickThrough(this, false);
            WindowHelper.MaximizeWindow(this);
            WindowHelper.SetExcludeFromCapture(this, true);

            var scale = WindowHelper.GetScaleFactor(this);
            var physicalBounds = DpiHelper.ToPhysical(new Windows.Foundation.Size(Bounds.Width, Bounds.Height), scale);
            FullAreaRect.Rect = new(0, 0, physicalBounds.Width, physicalBounds.Height);
        }
    }
}
