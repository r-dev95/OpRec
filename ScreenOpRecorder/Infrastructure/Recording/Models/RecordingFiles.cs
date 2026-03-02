using Windows.Storage;

namespace ScreenOpRecorder.Infrastructure.Recording.Models
{
    public sealed class RecordingFiles
    {
        public required StorageFile FinalFilePath { get; init; }
        public required StorageFile VideoFilePath { get; init; }
        public StorageFile? AudioFilePath { get; init; }
    }
}
