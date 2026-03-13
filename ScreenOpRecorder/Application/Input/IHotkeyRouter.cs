using System;
using System.Threading.Tasks;

namespace ScreenOpRecorder.Application.Input
{
    public interface IHotkeyRouter
    {
        void Register(HotkeyAction action, Func<Task> handler);

        Task<bool> TryHandleAsync(string keyName);
    }
}
