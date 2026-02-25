using System;
using System.IO;
using System.Threading.Tasks;

using ScreenOpRecorder.Core.Settings.Models;

using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class OutputCoordinator : IOutputCoordinator
    {
        public string? LastOutputFolderPath { get; private set; }

        public OutputCoordinator() { }

        public async Task PrepareAsync(OutputArtifacts artifacts, UserSettings settings)
        {
            string outputFolderPath = settings.OutputFolderPath;
            Directory.CreateDirectory(outputFolderPath);
            var localFolder = await StorageFolder.GetFolderFromPathAsync(outputFolderPath);
            var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

            var finalOutputFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            LastOutputFolderPath = localFolder.Path;

            artifacts.Reset();
            artifacts.FinalOutputFile = finalOutputFile;
            artifacts.VideoOutputFile = finalOutputFile;

            if (!settings.EnableAudioCapture)
            {
                return;
            }

            var baseName = Path.GetFileNameWithoutExtension(finalOutputFile.Name);
            var videoOutputFile = await localFolder.CreateFileAsync($"{baseName}.video.tmp.mp4", CreationCollisionOption.GenerateUniqueName);
            var audioOutputFile = await localFolder.CreateFileAsync($"{baseName}.audio.tmp.m4a", CreationCollisionOption.GenerateUniqueName);

            artifacts.CaptureAudio = true;
            artifacts.VideoOutputFile = videoOutputFile;
            artifacts.AudioOutputFile = audioOutputFile;
        }
    }
}
