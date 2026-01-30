using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

        public ShellPage(ILogger<ShellPage> logger, ShellViewModel viewModel)
        {
            InitializeComponent();
            _logger = logger;

            ViewModel = viewModel;
        }

        public void ResizeWindow(Window window)
        {
            // TODO: DPIスケーリング対応
            double scalingFactor = 2.0;

            RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _logger.LogDebug("RootGrid desired size width: {}, height: {}", RootGrid.DesiredSize.Width, RootGrid.DesiredSize.Height);

            var width = (int)((RootGrid.DesiredSize.Width + 40) * scalingFactor);
            var height = (int)((RootGrid.DesiredSize.Height + 60) * scalingFactor);
            _logger.LogDebug("Calculated window size width: {}, height: {}", width, height);

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow?.Resize(new SizeInt32(width, height));
        }
    }
}
