using System.Threading.Tasks;

namespace ScreenOpRecorder.Application.System.Interfaces
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}

