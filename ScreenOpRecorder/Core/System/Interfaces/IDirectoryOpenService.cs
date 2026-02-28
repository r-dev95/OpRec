using System.Threading.Tasks;

namespace ScreenOpRecorder.Core.System.Interfaces
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}

