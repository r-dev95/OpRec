using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IFolderOpenService
    {
        Task OpenAsync(string folderPath);
    }
}
