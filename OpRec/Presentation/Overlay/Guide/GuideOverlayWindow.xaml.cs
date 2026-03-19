using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

using OpRec.Common.Helpers;

namespace OpRec.Presentation.Overlay.Guide
{
    public sealed partial class GuideOverlayWindow : Window
    {
        private readonly ILogger _logger;
        private readonly GuideOverlayViewModel ViewModel;

        public GuideOverlayWindow(
            ILogger<GuideOverlayWindow> logger,
            GuideOverlayViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            SetWindow();

            Closed += OnClosed;
            ViewModel.SetRecordingUi += OnSetRecordingUi;
            ViewModel.UnSetRecordingUi += OnUnSetRecordingUi;
            ViewModel.Start(WindowHelper.GetScaleFactor(this), Bounds.Width, Bounds.Height);
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel.SetRecordingUi -= OnSetRecordingUi;
            ViewModel.UnSetRecordingUi -= OnUnSetRecordingUi;
            ViewModel.Stop();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint((UIElement)sender).Position;
            ViewModel.OnPointerPressed(point);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint((UIElement)sender).Position;
            ViewModel.OnPointerMoved(point);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.OnPointerReleased();
        }

        private void OnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, true);
            WindowHelper.SetClickThrough(this, true);
        }

        private void OnUnSetRecordingUi()
        {
            WindowHelper.SetAlwaysOnTop(this, false);
            WindowHelper.SetClickThrough(this, false);
        }

        private void SetWindow()
        {
            WindowHelper.SetBorderAndTitleBar(this, false, false);
            WindowHelper.SetClickThrough(this, false);
            WindowHelper.MaximizeWindow(this);
            WindowHelper.SetExcludeFromCapture(this, true);
        }
    }
}
