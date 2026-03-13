using System.Threading.Tasks;

namespace OpRec.Application.Recording
{
    public interface IStopRecordingUseCase
    {
        Task StopAsync();
    }
}
