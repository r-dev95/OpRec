using System.Threading.Tasks;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IMediaFileMerger
    {
        Task MergeAsync();
    }
}

