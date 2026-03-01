using Microsoft.Extensions.DependencyInjection;

using ScreenOpRecorder.Application.Input;
using ScreenOpRecorder.Application.Recording;
using ScreenOpRecorder.Application.Recording.State;

namespace ScreenOpRecorder.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IInputEventListener, InputEventListener>();
            services.AddSingleton<IRecordingSessionStore, RecordingSessionStore>();
            services.AddSingleton<ISelectCaptureAreaUseCase, SelectCaptureAreaUseCase>();
            services.AddSingleton<IStartRecordingUseCase, StartRecordingUseCase>();
            services.AddSingleton<IStopRecordingUseCase, StopRecordingUseCase>();
            services.AddHostedService<RecordingSessionZoomSyncHostedService>();

            return services;
        }
    }
}
