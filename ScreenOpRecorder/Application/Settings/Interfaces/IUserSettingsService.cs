using System;
using System.Threading.Tasks;

using ScreenOpRecorder.Application.Settings.Models;

namespace ScreenOpRecorder.Application.Settings.Interfaces
{
    public interface IUserSettingsService
    {
        UserSettings Current { get; }

        event Action<UserSettings>? SettingsChanged;

        Task SaveAsync(UserSettings settings);
    }
}
