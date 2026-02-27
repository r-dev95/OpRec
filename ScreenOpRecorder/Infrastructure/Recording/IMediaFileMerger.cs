using System.Threading.Tasks;

using ScreenOpRecorder.Infrastructure.Recording.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IMediaFileMerger
    {
        Task MergeAsync(RecordingFiles files);
    }
}

