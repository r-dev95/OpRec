using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording
{
    public interface ISelectCaptureAreaUseCase
    {
        bool SelectCaptureArea(ScreenRect captureArea);
    }
}
