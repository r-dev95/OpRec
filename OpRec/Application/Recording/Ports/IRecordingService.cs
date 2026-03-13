using System.Threading.Tasks;

using OpRec.Domain.ValueObjects;

namespace OpRec.Application.Recording.Ports
{
    public interface IRecordingService
    {
        bool TrySelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();

        bool TryToggleZoomAt(int screenX, int screenY);
    }
}
