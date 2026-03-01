using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Application.Recording.Events
{
    public record ZoomAreaChangedEvent(ScreenRect ZoomRect);
}
