using System.Threading.Tasks;

namespace OpRec.Application.Recording
{
    public interface IStartRecordingUseCase
    {
        Task<bool> StartAsync();
    }
}
