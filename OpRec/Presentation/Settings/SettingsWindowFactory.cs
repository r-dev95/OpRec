using System;

using Microsoft.Extensions.DependencyInjection;

namespace OpRec.Presentation.Settings
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
