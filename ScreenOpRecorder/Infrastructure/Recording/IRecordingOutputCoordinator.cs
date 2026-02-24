using System.Threading.Tasks;

using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IRecordingOutputCoordinator
    {
        string? LastOutputFolderPath { get; }

        Task PrepareAsync(RecordingOutputArtifacts artifacts, UserSettings settings);
    }
}
