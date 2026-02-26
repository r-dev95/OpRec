using System;
using System.Threading.Tasks;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IFileManager : IDisposable
    {
        FileManager.FilePathList FileList { get; }

        Task<bool> SetupAsync();

        void Reset();
    }
}
