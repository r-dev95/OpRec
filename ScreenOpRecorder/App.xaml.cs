using System;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

using NLog.Extensions.Logging;

using ScreenOpRecorder.Features.Input;
using ScreenOpRecorder.Features.Overlay;
using ScreenOpRecorder.Features.Record;
using ScreenOpRecorder.Features.Shell;

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
                    services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ShellPage>();
                    services.AddSingleton<ShellViewModel>();
                    services.AddSingleton<OverlayWindow>();
                    services.AddSingleton<OverlayViewModel>();

                    // Input Hook Services
                    services.AddSingleton<MouseHookService>();
                    services.AddSingleton<KeyboardHookService>();
                    services.AddSingleton<RecordService>();

                })
                .Build();
        }

        public static T GetService<T>()
            where T : class
        {
            if ((Current as App)!._host.Services.GetService(typeof(T)) is not T service)
            {
                string msg = $"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.";
                throw new ArgumentException(msg);
            }

            return service;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var shellPage = GetService<ShellPage>();

            _mainWindow = GetService<MainWindow>();
            _mainWindow.Content = shellPage;
            _mainWindow.Activate();

            _overlayWindow = GetService<OverlayWindow>();
            _overlayWindow.Activate();

            _mainWindow.Closed += async (_, _) =>
            {
                await shellPage.StopRecordingAsync();
                _overlayWindow.Close();
            };
        }
    }
}
