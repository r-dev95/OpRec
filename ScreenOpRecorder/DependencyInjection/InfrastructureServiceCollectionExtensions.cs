using Microsoft.Extensions.DependencyInjection;

using ScreenOpRecorder.Core.Events.Interfaces;
using ScreenOpRecorder.Core.Input.Interfaces;
using ScreenOpRecorder.Core.Recording.Interfaces;
using ScreenOpRecorder.Core.Settings.Interfaces;
using ScreenOpRecorder.Core.System.Interfaces;
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
