using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.Recording.Ports
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}
