using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.Recording
{
    public interface IStopRecordingUseCase
    {
        Task StopAsync();
    }
}
