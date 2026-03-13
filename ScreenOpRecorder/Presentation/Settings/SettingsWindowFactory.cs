using System;

using Microsoft.Extensions.DependencyInjection;

namespace ScreenOpRecorder.Presentation.Settings
{
    public sealed class SettingsWindowFactory : ISettingsWindowFactory
    {
        private readonly IServiceProvider _services;

        public SettingsWindowFactory(IServiceProvider services)
        {
            _services = services;
        }

        public SettingsWindow Create()
        {
            return _services.GetRequiredService<SettingsWindow>();
        }
    }
}
