using System;
using System.Threading.Tasks;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IAudioCaptureService : IDisposable
    {
        Task<bool> StartAsync(StorageFile filePath);

        Task StopAsync();
    }
}
