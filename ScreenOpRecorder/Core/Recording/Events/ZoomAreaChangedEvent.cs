using ScreenOpRecorder.Domain.ValueObjects;

namespace ScreenOpRecorder.Core.Recording.Events
{
    public record ZoomAreaChangedEvent(ScreenRect ZoomRect);
}
