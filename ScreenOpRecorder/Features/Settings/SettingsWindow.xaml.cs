using System;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using ScreenOpRecorder.Shared.Helpers;

using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage.Pickers;

using WinRT.Interop;

namespace ScreenOpRecorder.Features.Settings
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

            ViewModel.CloseRequested += OnCloseRequested;

            Closed += (_, _) => ViewModel.CloseRequested -= OnCloseRequested;

            SetWindow();
        }

        private void OnCloseRequested()
        {
            Close();
        }

        private async void OnClickBrowseFolder(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.SetOutputFolderPath(folder.Path);
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
