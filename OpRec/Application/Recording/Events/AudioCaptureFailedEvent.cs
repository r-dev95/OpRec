using System;

namespace OpRec.Application.Recording.Events
{
    public sealed record AudioCaptureFailedEvent(string Reason, Exception? Exception);
}
