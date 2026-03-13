using System.Threading.Tasks;

namespace OpRec.Application.System.Ports
{
    public interface IDirectoryOpenService
    {
        Task OpenAsync(string dirPath);
    }
}

