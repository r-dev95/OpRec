using System;
using System.Threading.Tasks;

using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Application.Settings.Ports
{
    public interface IUserSettingsService
    {
        UserSettings Current { get; }

        event Action<UserSettings>? SettingsChanged;

        Task SaveAsync(UserSettings settings);
    }
}
