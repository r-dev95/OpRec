using System;
using System.Threading.Tasks;

using ScreenOpRecorder.Domain.ValueObjects;
using ScreenOpRecorder.Infrastructure.Recording.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IDisplayCaptureService : IDisposable
    {
        bool HasSelectedCaptureArea { get; }

        event Action<RecordingState>? RecordingStateChanged;
        event Action<ScreenRect>? ZoomAreaChanged;

        bool TrySelectCaptureArea(ScreenRect captureArea);

        Task<bool> StartAsync();

        Task StopAsync();
    }
}

