using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using NLog.Extensions.Logging;

using ScreenOpRecorder.DependencyInjection;
using ScreenOpRecorder.Presentation.Overlay.Guide;
using ScreenOpRecorder.Presentation.Overlay.Recording;
using ScreenOpRecorder.Presentation.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Microsoft.UI.Xaml.Application
    {
        private readonly IHost _host;
        private MainWindow? _mainWindow;
        private RecordingOverlayWindow? _recordingOverlayWindow;
        private GuideOverlayWindow? _guideOverlayWindow;

        public App()
        {
            InitializeComponent();

            _host = Host
                .CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddDebug();
                    logging.AddNLog();
                    logging.AddConfiguration(context.Configuration);
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddPresentationServices()
                        .AddApplicationServices()
                        .AddInfrastructureServices();
                })
                .Build();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _host.Start();

            _mainWindow = _host.Services.GetRequiredService<MainWindow>();
            _mainWindow.Content = _host.Services.GetRequiredService<ShellPage>();
            _mainWindow.Closed += OnMainWindowClosed;
            _mainWindow.Activate();

            _recordingOverlayWindow = _host.Services.GetRequiredService<RecordingOverlayWindow>();
            _recordingOverlayWindow.Activate();

            _guideOverlayWindow = _host.Services.GetRequiredService<GuideOverlayWindow>();
            _guideOverlayWindow.Activate();
        }

        private void OnMainWindowClosed(object sender, WindowEventArgs args)
        {
            _mainWindow?.Closed -= OnMainWindowClosed;
            _recordingOverlayWindow?.Close();
            _guideOverlayWindow?.Close();
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}
