using System;
using System.Threading.Tasks;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IAudioCaptureService : IDisposable
    {
        Task<bool> StartAsync();

        Task StopAsync();
    }
}
