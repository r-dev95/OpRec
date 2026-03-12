using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.Ports
{
    public interface IRecordingService
    {
        bool TrySelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
