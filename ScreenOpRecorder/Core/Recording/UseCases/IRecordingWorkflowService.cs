using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.UseCases
{
    public interface IRecordingWorkflowService
    {
        bool SelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}
