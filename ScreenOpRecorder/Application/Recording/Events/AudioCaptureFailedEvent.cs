using System;

namespace ScreenOpRecorder.Application.Recording.Events
{
    public sealed record AudioCaptureFailedEvent(string Reason, Exception? Exception);
}
