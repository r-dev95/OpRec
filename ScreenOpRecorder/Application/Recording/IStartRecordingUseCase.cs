using System.Threading.Tasks;

namespace ScreenOpRecorder.Application.Recording
{
    public interface IStartRecordingUseCase
    {
        Task<bool> StartAsync();
    }
}
