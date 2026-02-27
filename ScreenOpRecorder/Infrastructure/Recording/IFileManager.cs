using System.Threading.Tasks;

using ScreenOpRecorder.Infrastructure.Recording.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IFileManager
    {
        Task<RecordingFiles?> SetupAsync();

        Task CleanupRecordingFilesAsync(RecordingFiles? files);
    }
}
