using Microsoft.Extensions.DependencyInjection;
using WeaponMenu.Core.Configuration;
using WeaponMenu.Core.Modules;

namespace WeaponMenu.Core;

internal static class ModuleDependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddModuleDi()
        {
            services.AddSingleton<WeaponMenuConfig>();

            services.AddSingleton<SharedInterfaceModule>();
            services.AddSingleton<IModule>(sp => sp.GetRequiredService<SharedInterfaceModule>());

            services.AddSingleton<RoundTrackerModule>();
            services.AddSingleton<IModule>(sp => sp.GetRequiredService<RoundTrackerModule>());

            services.AddSingleton<IModule, WeaponMenuModule>();
        }
    }
}
