using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.Recording
{
    public interface IStartRecordingUseCase
    {
        Task<bool> StartAsync();
    }
}
