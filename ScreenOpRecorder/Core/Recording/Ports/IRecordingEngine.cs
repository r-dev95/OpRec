using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IRecordingEngine
    {
        string? LastOutputFolderPath { get; }

        bool TrySelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
