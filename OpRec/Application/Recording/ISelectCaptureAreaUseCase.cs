using OpRec.Domain.ValueObjects;

namespace OpRec.Application.Recording
{
    public interface ISelectCaptureAreaUseCase
    {
        bool SelectCaptureArea(ScreenRect captureArea);
    }
}
