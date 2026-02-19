using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using ScreenOpRecorder.Shared.Helpers;

using Windows.Foundation;
using Windows.Graphics;

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

            ViewModel.StartRecord += OnStartRecord;
            ViewModel.StopRecord += OnStopRecord;

            SetWindow();
        }

        private async void OnClickClose(object sender, RoutedEventArgs args)
        {
            await ViewModel.StopRecordingAsync();

            ViewModel.StartRecord -= OnStartRecord;
            ViewModel.StopRecord -= OnStopRecord;

            _mainWindow.Close();
        }

        private void OnStartRecord()
        {
            WindowHelper.SetAlwaysOnTop(_mainWindow, false);
            RecordingButton.Icon = new SymbolIcon(Symbol.Stop);
            RecordingButton.Label = "Stop";
        }

        private void OnStopRecord()
        {
            WindowHelper.SetAlwaysOnTop(_mainWindow, true);
            RecordingButton.Icon = new SymbolIcon(Symbol.Video);
            RecordingButton.Label = "Recording";
        }

        private void SetWindow()
        {
            WindowHelper.SetAlwaysOnTop(_mainWindow, true);

            _mainWindow.ExtendsContentIntoTitleBar = true;
            WindowHelper.SetBorderAndTitleBar(_mainWindow, false, false);

            RootPage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _logger.LogDebug("RootPage desired size width: {}, height: {}", RootPage.DesiredSize.Width, RootPage.DesiredSize.Height);
            var scalingFactor = WindowHelper.GetScaleFactor(_mainWindow);
            var width = DpiHelper.ToPhysicalInt(RootPage.DesiredSize.Width, scalingFactor);
            var height = DpiHelper.ToPhysicalInt(RootPage.DesiredSize.Height, scalingFactor);
            _logger.LogDebug("Calculated window size width: {}, height: {}", width, height);

            var displayArea = WindowHelper.GetDisplayArea(_mainWindow, DisplayAreaFallback.Nearest);
            var screenBounds = displayArea.WorkArea;
            int x = screenBounds.X + (screenBounds.Width - width) / 2;
            int y = screenBounds.Y + 20;
            WindowHelper.MoveAndResize(_mainWindow, new RectInt32(x, y, width, height));
        }
    }
}
