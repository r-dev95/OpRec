using Microsoft.Extensions.DependencyInjection;

using OpRec.Application.Input;
using OpRec.Application.Recording;
using OpRec.Application.Recording.Session;

namespace OpRec.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IHotkeyRouter, HotkeyRouter>();
            services.AddSingleton<IInputEventListener, InputEventListener>();
            services.AddSingleton<IRecordingSessionStore, RecordingSessionStore>();
            services.AddSingleton<ISelectCaptureAreaUseCase, SelectCaptureAreaUseCase>();
            services.AddSingleton<IStartRecordingUseCase, StartRecordingUseCase>();
            services.AddSingleton<IStopRecordingUseCase, StopRecordingUseCase>();
            services.AddSingleton<IToggleZoomAtCursorUseCase, ToggleZoomAtCursorUseCase>();
            services.AddHostedService<RecordingSessionZoomSyncHostedService>();

            return services;
        }
    }
}
