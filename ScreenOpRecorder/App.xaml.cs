using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using NLog.Extensions.Logging;

using ScreenOpRecorder.Core.Events.Ports;
using ScreenOpRecorder.Core.Input;
using ScreenOpRecorder.Core.Input.Ports;
using ScreenOpRecorder.Core.Recording;
using ScreenOpRecorder.Core.Recording.Ports;
using ScreenOpRecorder.Core.Recording.State;
using ScreenOpRecorder.Core.Settings.Ports;
using ScreenOpRecorder.Core.System.Ports;
using ScreenOpRecorder.Infrastructure.Events;
using ScreenOpRecorder.Infrastructure.Input;
using ScreenOpRecorder.Infrastructure.Recording;
using ScreenOpRecorder.Infrastructure.Settings;
using ScreenOpRecorder.Infrastructure.System;
using ScreenOpRecorder.Presentation.Overlay;
using ScreenOpRecorder.Presentation.Settings;
using ScreenOpRecorder.Presentation.Shell;

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
                    // Presentation
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ShellPage>();
                    services.AddSingleton<ShellViewModel>();
                    services.AddSingleton<OverlayWindow>();
                    services.AddSingleton<OverlayViewModel>();
                    services.AddTransient<SettingsWindow>();
                    services.AddTransient<SettingsViewModel>();

                    // Core
                    services.AddSingleton<IInputHookService, InputHookService>();
                    services.AddSingleton<IRecordingUseCase, RecordingUseCase>();
                    services.AddSingleton<IRecordingSessionStore, RecordingSessionStore>();

                    // Infrastructure.Events
                    services.AddSingleton<IEventBus, EventBus>();

                    // Infrastructure.System
                    services.AddSingleton<IDirectoryOpenService, DirectoryOpenService>();

                    // Infrastructure.Settings
                    services.AddSingleton<IUserSettingsService, UserSettingsService>();

                    // Infrastructure.Input
                    services.AddSingleton<IMouseHookService, MouseHookService>();
                    services.AddSingleton<IKeyboardHookService, KeyboardHookService>();

                    // Infrastructure.Recording
                    services.AddSingleton<IRecordingService, RecordingService>();
                    services.AddSingleton<FileManager>();
                    services.AddSingleton<DisplayCaptureService>();
                    services.AddSingleton<AudioCaptureService>();
                    services.AddSingleton<MediaFileMerger>();
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
            _host.Dispose();
        }
    }
}

