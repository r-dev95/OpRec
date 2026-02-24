using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IOutputFolderOpener
    {
        Task OpenAsync(string folderPath);
    }
}
