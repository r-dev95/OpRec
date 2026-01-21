using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using RecordingApp.Features.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RecordingApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private Window? _window;

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
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<MainWindow>();
                    services.AddSingleton<ShellPage>();
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
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = GetService<MainWindow>();
            _window.Content = GetService<ShellPage>();
            _window.Activate();
        }
    }
}
