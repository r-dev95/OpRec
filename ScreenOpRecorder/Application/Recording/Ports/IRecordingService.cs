using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.Ports
{
    public interface IRecordingService
    {
        string? LastOutputDirPath { get; }

        bool TrySelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
