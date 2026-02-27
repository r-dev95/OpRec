using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.System.Ports
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}

