using System;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Common.Helpers;

using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage.Pickers;

using WinRT.Interop;

namespace ScreenOpRecorder.Presentation.Settings
{
    public sealed partial class SettingsWindow : Window
    {
        private readonly ILogger<SettingsWindow> _logger;
        private readonly SettingsViewModel ViewModel;

        public SettingsWindow(ILogger<SettingsWindow> logger, SettingsViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;

            Closed += OnClosed;
            ViewModel.CloseRequested += OnCloseRequested;

            //SetWindow();
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel.CloseRequested -= OnCloseRequested;
        }

        private void OnCloseRequested()
        {
            Close();
        }

        private async void OnClickBrowseDirectory(object sender, RoutedEventArgs args)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var hwnd = WindowHelper.GetHwnd(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            var dir = await picker.PickSingleFolderAsync();
            if (dir != null)
            {
                ViewModel.SetOutputDirPath(dir.Path);
            }
        }

        private void SetWindow()
        {
            RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _logger.LogDebug("RootGrid desired size width: {}, height: {}", RootGrid.DesiredSize.Width, RootGrid.DesiredSize.Height);
            var scalingFactor = WindowHelper.GetScaleFactor(this);
            var width = DpiHelper.ToPhysicalInt(RootGrid.DesiredSize.Width, scalingFactor);
            var height = DpiHelper.ToPhysicalInt(RootGrid.DesiredSize.Height, scalingFactor);
            _logger.LogDebug("Calculated window size width: {}, height: {}", width, height);

            var displayArea = WindowHelper.GetDisplayArea(this, DisplayAreaFallback.Nearest);
            var screenBounds = displayArea.WorkArea;
            int x = screenBounds.X + (screenBounds.Width - width) / 2;
            int y = screenBounds.Y + 150;
            WindowHelper.MoveAndResize(this, new RectInt32(x, y, width, height));
        }
    }
}
