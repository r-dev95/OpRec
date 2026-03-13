using OpRec.Domain.ValueObjects;

namespace OpRec.Application.Recording.Events
{
    public record ZoomAreaChangedEvent(ScreenRect ZoomRect);
}
