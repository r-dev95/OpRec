using Microsoft.Extensions.DependencyInjection;

using ScreenOpRecorder.Application.Events.Interfaces;
using ScreenOpRecorder.Application.Input.Interfaces;
using ScreenOpRecorder.Application.Recording.Interfaces;
using ScreenOpRecorder.Application.Settings.Interfaces;
using ScreenOpRecorder.Application.System.Interfaces;
using ScreenOpRecorder.Infrastructure.Events;
using ScreenOpRecorder.Infrastructure.Input;
using ScreenOpRecorder.Infrastructure.Recording;
using ScreenOpRecorder.Infrastructure.Settings;
using ScreenOpRecorder.Infrastructure.System;

namespace ScreenOpRecorder.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<IDirectoryOpenService, DirectoryOpenService>();
            services.AddSingleton<IUserSettingsService, UserSettingsService>();
            services.AddSingleton<IMouseInputListener, MouseInputListener>();
            services.AddSingleton<IKeyboardInputListener, KeyboardInputListener>();
            services.AddSingleton<IRecordingService, RecordingService>();
            services.AddSingleton<FileManager>();
            services.AddSingleton<DisplayCaptureService>();
            services.AddSingleton<AudioCaptureService>();
            services.AddSingleton<MediaFileMerger>();

            return services;
        }
    }
}
