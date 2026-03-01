using System.Threading.Tasks;

namespace ScreenOpRecorder.Application.Recording
{
    public interface IStopRecordingUseCase
    {
        Task StopAsync();
    }
}
