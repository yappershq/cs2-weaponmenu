using Microsoft.Extensions.Logging;
using Sharp.Shared.Objects;
using WeaponMenu.Shared;

namespace WeaponMenu.Core.Modules;

internal sealed class SharedInterfaceModule : IModule, IWeaponMenuShared
{
    private readonly InterfaceBridge                  _bridge;
    private readonly ILogger<SharedInterfaceModule>   _logger;

    public event System.Func<IGameClient, bool>? CanUseFor;

    public SharedInterfaceModule(InterfaceBridge bridge, ILogger<SharedInterfaceModule> logger)
    {
        _bridge = bridge;
        _logger = logger;
    }

    public bool Init() => true;

    public void OnPostInit(System.IServiceProvider provider)
    {
        _bridge.SharpModuleManager.RegisterSharpModuleInterface<IWeaponMenuShared>(
            _bridge.Module, IWeaponMenuShared.Identity, this);
        _logger.LogInformation("[WeaponMenu] Registered IWeaponMenuShared ({Id})", IWeaponMenuShared.Identity);
    }

    internal bool ShouldAllow(IGameClient client)
    {
        var handlers = CanUseFor;
        if (handlers is null)
            return true;

        foreach (var d in handlers.GetInvocationList())
        {
            if (d is System.Func<IGameClient, bool> fn && !fn(client))
                return false;
        }
        return true;
    }
}
