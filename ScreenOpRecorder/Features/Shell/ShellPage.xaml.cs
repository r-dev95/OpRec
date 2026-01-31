using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;

using ScreenOpRecorder.Features.Overlay;

using Windows.Foundation;
using Windows.Graphics;

using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        private readonly ILogger<ShellPage> _logger;
        private readonly ShellViewModel ViewModel;
        private readonly MainWindow _mainWindow;

        public ShellPage(ILogger<ShellPage> logger, ShellViewModel viewModel, MainWindow mainWindow)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;
            _mainWindow = mainWindow;

            ResizeWindow();

            ViewModel.StartRecord += OnStartRecord;
            ViewModel.StopRecord += OnStopRecord;
        }

        public async Task StopRecordingAsync()
        {
            await ViewModel.StopRecordingAsync();
        }

        private void ResizeWindow()
        {
            var scalingFactor = OverlayHelper.GetScaleFactor(_mainWindow);

            RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _logger.LogDebug("RootGrid desired size width: {}, height: {}", RootGrid.DesiredSize.Width, RootGrid.DesiredSize.Height);

            var width = (int)((RootGrid.DesiredSize.Width + 40) * scalingFactor);
            var height = (int)((RootGrid.DesiredSize.Height + 60) * scalingFactor);
            _logger.LogDebug("Calculated window size width: {}, height: {}", width, height);

            IntPtr hWnd = WindowNative.GetWindowHandle(_mainWindow);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // 現在のディスプレイ情報を取得
                DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
                var screenBounds = displayArea.WorkArea;

                int x = screenBounds.X + (screenBounds.Width - width) / 2;
                int y = screenBounds.Y;

                appWindow.MoveAndResize(new RectInt32(x, y, width, height));
            }

            OverlayHelper.SetAlwaysOnTop(_mainWindow, true);
        }

        private void OnStartRecord()
        {
            OverlayHelper.SetAlwaysOnTop(_mainWindow, false);
        }

        private void OnStopRecord()
        {
            OverlayHelper.SetAlwaysOnTop(_mainWindow, true);
        }
    }
}
