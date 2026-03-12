using Microsoft.Extensions.DependencyInjection;

using ScreenOpRecorder.Application.Events.Ports;
using ScreenOpRecorder.Application.Input.Ports;
using ScreenOpRecorder.Application.Recording.Ports;
using ScreenOpRecorder.Application.Settings.Ports;
using ScreenOpRecorder.Application.System.Ports;
using ScreenOpRecorder.Infrastructure.Events;
using ScreenOpRecorder.Infrastructure.Input;
using ScreenOpRecorder.Infrastructure.Recording;
using ScreenOpRecorder.Infrastructure.Recording.Audio;
using ScreenOpRecorder.Infrastructure.Recording.Video;
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
            services.AddSingleton<MediaFileMerger>();
            services.AddSingleton<VideoCapture>();
            services.AddSingleton<AudioCapture>();
            services.AddSingleton<MicAudioCapture>();
            services.AddSingleton<SystemAudioCapture>();
            services.AddSingleton<AudioMixer>();
            services.AddSingleton<AudioTranscoder>();

            return services;
        }
    }
}
