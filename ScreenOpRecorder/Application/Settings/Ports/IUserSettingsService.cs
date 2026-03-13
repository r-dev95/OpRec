using System;
using System.Threading.Tasks;

using ScreenOpRecorder.Domain.Settings.ValueObjects;

namespace ScreenOpRecorder.Application.Settings.Ports
{
    public interface IUserSettingsService
    {
        UserSettings Current { get; }

        event Action<UserSettings>? SettingsChanged;

        Task SaveAsync(UserSettings settings);
    }
}
