using System;

using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder.Features.Overlay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayPage : Page
    {
        private readonly ILogger _logger;

        public OverlayPage(ILogger<OverlayPage> logger, MainWindow mainWindow)
        {
            InitializeComponent();
            _logger = logger;

            mainWindow.ExtendsContentIntoTitleBar = true;
            OverlayHelper.SetAlwaysOnTop(mainWindow, true);
            OverlayHelper.SetClickThrough(mainWindow, true);
            OverlayHelper.SetWindowOpacity(mainWindow, 128);
            OverlayHelper.MaximizeWindow(mainWindow);
        }
    }
}
