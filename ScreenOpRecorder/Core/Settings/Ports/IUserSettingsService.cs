using System;
using System.Threading.Tasks;

using ScreenOpRecorder.Core.Settings.Models;

namespace ScreenOpRecorder.Core.Settings.Ports
{
    public interface IUserSettingsService
    {
        UserSettings Current { get; }

        event Action<UserSettings>? SettingsChanged;

        Task SaveAsync(UserSettings settings);
    }
}
