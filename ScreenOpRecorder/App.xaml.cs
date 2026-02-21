using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using NLog.Extensions.Logging;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Overlay;
using ScreenOpRecorder.Features.Record;
using ScreenOpRecorder.Features.Record.State;
using ScreenOpRecorder.Features.Settings;
using ScreenOpRecorder.Features.Shell;
using ScreenOpRecorder.Shared.Events;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenOpRecorder
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private MainWindow? _mainWindow;
        private OverlayWindow? _overlayWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
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
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ShellPage>();
                    services.AddSingleton<ShellViewModel>();
                    services.AddSingleton<OverlayWindow>();
                    services.AddSingleton<OverlayViewModel>();
                    services.AddTransient<SettingsWindow>();
                    services.AddTransient<SettingsViewModel>();

                    // Services
                    services.AddSingleton<MouseHookService>();
                    services.AddSingleton<KeyboardHookService>();
                    services.AddSingleton<RecordService>();
                    services.AddSingleton<IEventBus, EventBus>();
                    services.AddSingleton<IRecordingStateStore, RecordingStateStore>();
                    services.AddSingleton<IRecordingDomainService, RecordingDomainService>();
                    services.AddSingleton<IUserSettingsService, UserSettingsService>();

                })
                .Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _mainWindow = _host.Services.GetRequiredService<MainWindow>();
            _mainWindow.Content = _host.Services.GetRequiredService<ShellPage>();
            _mainWindow.Activate();

            _overlayWindow = _host.Services.GetRequiredService<OverlayWindow>();
            _overlayWindow.Activate();

            _mainWindow.Closed += OnMainWindowClosed;
        }

        private void OnMainWindowClosed(object sender, WindowEventArgs args)
        {
            _mainWindow?.Closed -= OnMainWindowClosed;
            _overlayWindow?.Close();
        }
    }
}
