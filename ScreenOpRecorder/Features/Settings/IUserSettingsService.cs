using System;
using System.Threading.Tasks;

namespace ScreenOpRecorder.Features.Settings
{
    public interface IUserSettingsService
    {
        UserSettings Current { get; }

        event Action<UserSettings>? SettingsChanged;

        Task SaveAsync(UserSettings settings);
    }
}
