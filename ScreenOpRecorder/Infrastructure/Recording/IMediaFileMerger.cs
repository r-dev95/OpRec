using System.Threading.Tasks;

using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IMediaFileMerger
    {
        Task MergeIfNeededAsync(OutputArtifacts artifacts, QualityPreset qualityPreset, int recordingFps);
    }
}
