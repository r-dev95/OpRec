using System.Threading.Tasks;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public interface IRecordingDomainService
    {
        bool SelectCaptureArea(Rect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
