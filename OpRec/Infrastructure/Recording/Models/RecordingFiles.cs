using System.Collections.Generic;

using Windows.Storage;

namespace OpRec.Infrastructure.Recording.Models
{
    public sealed class RecordingFiles
    {
        public required StorageFile FinalFile { get; init; }
        public required StorageFile VideoFile { get; init; }
        public StorageFile? AudioFile { get; init; }
        public StorageFile? MicTempFile { get; set; }
        public StorageFile? SystemTempFile { get; set; }
        public List<StorageFile> AudioTempFiles { get; } = new();
    }
}
