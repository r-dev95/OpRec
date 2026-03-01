using System.Threading.Tasks;

namespace ScreenOpRecorder.Application.System.Ports
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}

