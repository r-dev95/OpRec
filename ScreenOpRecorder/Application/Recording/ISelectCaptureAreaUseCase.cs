using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording
{
    public interface ISelectCaptureAreaUseCase
    {
        bool SelectCaptureArea(ScreenRect captureArea);
    }
}
