using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using OpRec.Common.Helpers;
using OpRec.Presentation.Settings;

using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OpRec.Presentation.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        private readonly ILogger<ShellPage> _logger;
        private readonly ShellViewModel ViewModel;
        private readonly MainWindow _mainWindow;
        private readonly ISettingsWindowFactory _settingsWindowFactory;
        private SettingsWindow? _settingsWindow;

        public ShellPage(
            ILogger<ShellPage> logger,
            ShellViewModel viewModel,
            MainWindow mainWindow,
            ISettingsWindowFactory settingsWindowFactory)
        {
            InitializeComponent();
            _logger = logger;
            ViewModel = viewModel;
            _mainWindow = mainWindow;
            _settingsWindowFactory = settingsWindowFactory;

            ViewModel.StartRecord += OnStartRecord;
            ViewModel.StopRecord += OnStopRecord;
            ViewModel.Start();

            SetWindow();
        }

        private async void OnClickClose(object sender, RoutedEventArgs args)
        {
            await ViewModel.StopAsync();

            ViewModel.StartRecord -= OnStartRecord;
            ViewModel.StopRecord -= OnStopRecord;

            _settingsWindow?.Close();
            _mainWindow.Close();
        }

        private void OnClickSetting(object sender, RoutedEventArgs args)
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = _settingsWindowFactory.Create();
                _settingsWindow.Closed += OnSettingsWindowClosed;
            }

            _settingsWindow.Activate();
        }

        private void OnSettingsWindowClosed(object sender, WindowEventArgs args)
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Closed -= OnSettingsWindowClosed;
                _settingsWindow = null;
            }
        }

        private void OnStartRecord()
        {
            RecordingButton.Icon = new SymbolIcon(Symbol.Stop);
            RecordingButton.Label = "Stop";
        }

        private void OnStopRecord()
        {
            RecordingButton.Icon = new SymbolIcon(Symbol.Video);
            RecordingButton.Label = "Recording";
        }

        private void SetWindow()
        {
            WindowHelper.GetAppWindow(_mainWindow).SetIcon("Assets/icon.ico");
            WindowHelper.SetBorderAndTitleBar(_mainWindow, false, false);
            WindowHelper.SetExcludeFromCapture(_mainWindow, true);
            WindowHelper.SetAlwaysOnTop(_mainWindow, true);

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
            WindowHelper.MoveAndResize(_mainWindow, x, y, width, height);
        }
    }
}
