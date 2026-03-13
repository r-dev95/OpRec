using Microsoft.Extensions.DependencyInjection;

using OpRec.Presentation.Overlay.Guide;
using OpRec.Presentation.Overlay.Recording;
using OpRec.Presentation.Settings;
using OpRec.Presentation.Shell;

namespace OpRec.DependencyInjection
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
