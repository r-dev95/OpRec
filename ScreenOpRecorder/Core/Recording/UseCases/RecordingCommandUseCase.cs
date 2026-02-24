using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.UseCases
{
    public sealed class RecordingCommandUseCase : IRecordingCommandUseCase
    {
        private readonly IRecordingWorkflowService _recordingWorkflowService;

        public RecordingCommandUseCase(IRecordingWorkflowService recordingWorkflowService)
        {
            _recordingWorkflowService = recordingWorkflowService;
        }

        public bool SelectCaptureArea(ScreenRect captureArea)
        {
            if (!captureArea.HasArea)
            {
                return false;
            }

            return _recordingWorkflowService.SelectCaptureArea(captureArea);
        }

        public Task<bool> StartAsync()
        {
            return _recordingWorkflowService.StartAsync();
        }

        public Task StopAsync()
        {
            return _recordingWorkflowService.StopAsync();
        }
    }
}
