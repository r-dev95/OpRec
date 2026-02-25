using System.Threading.Tasks;

using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public interface IOutputCoordinator
    {
        string? LastOutputFolderPath { get; }

        Task PrepareAsync(OutputArtifacts artifacts, UserSettings settings);
    }
}
