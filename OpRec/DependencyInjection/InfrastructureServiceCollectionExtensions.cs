using Microsoft.Extensions.DependencyInjection;

using OpRec.Application.Events.Ports;
using OpRec.Application.Input.Ports;
using OpRec.Application.Recording.Ports;
using OpRec.Application.Settings.Ports;
using OpRec.Application.System.Ports;
using OpRec.Infrastructure.Events;
using OpRec.Infrastructure.Input;
using OpRec.Infrastructure.Recording;
using OpRec.Infrastructure.Recording.Audio;
using OpRec.Infrastructure.Recording.Video;
using OpRec.Infrastructure.Settings;
using OpRec.Infrastructure.System;

namespace OpRec.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<IUserSettingsService, UserSettingsService>();
            services.AddSingleton<IDirectoryOpenService, DirectoryOpenService>();
            services.AddSingleton<ICursorPositionService, CursorPositionService>();
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
