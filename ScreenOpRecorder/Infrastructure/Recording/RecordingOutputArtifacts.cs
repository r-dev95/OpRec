using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public sealed class RecordingOutputArtifacts
    {
        public bool CaptureAudio { get; set; }
        public StorageFile? VideoOutputFile { get; set; }
        public StorageFile? AudioOutputFile { get; set; }
        public StorageFile? FinalOutputFile { get; set; }

        public void Reset()
        {
            CaptureAudio = false;
            VideoOutputFile = null;
            AudioOutputFile = null;
            FinalOutputFile = null;
        }
    }
}
