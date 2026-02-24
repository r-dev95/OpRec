using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.UseCases
{
    public interface IRecordingCommandUseCase
    {
        bool SelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
