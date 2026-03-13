using Microsoft.Extensions.DependencyInjection;

using ScreenOpRecorder.Presentation.Overlay.Guide;
using ScreenOpRecorder.Presentation.Overlay.Recording;
using ScreenOpRecorder.Presentation.Settings;
using ScreenOpRecorder.Presentation.Shell;

namespace ScreenOpRecorder.DependencyInjection
{
    public static class PresentationServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ShellPage>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<RecordingOverlayWindow>();
            services.AddSingleton<GuideOverlayWindow>();
            services.AddSingleton<RecordingOverlayViewModel>();
            services.AddSingleton<GuideOverlayViewModel>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<ISettingsWindowFactory, SettingsWindowFactory>();

            return services;
        }
    }
}
