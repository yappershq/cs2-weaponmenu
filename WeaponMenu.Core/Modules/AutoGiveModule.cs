using Microsoft.Extensions.Logging;
using Sharp.Shared.HookParams;
using WeaponMenu.Core.Configuration;
using WeaponMenu.Core.Utils;

namespace WeaponMenu.Core.Modules;

internal sealed class AutoGiveModule : IModule
{
    private readonly InterfaceBridge            _bridge;
    private readonly WeaponMenuConfig           _config;
    private readonly RoundTrackerModule         _tracker;
    private readonly SharedInterfaceModule      _shared;
    private readonly ILogger<AutoGiveModule>    _logger;

    private readonly System.Action<IPlayerSpawnForwardParams> _onSpawn;

    public AutoGiveModule(
        InterfaceBridge            bridge,
        WeaponMenuConfig           config,
        RoundTrackerModule         tracker,
        SharedInterfaceModule      shared,
        ILogger<AutoGiveModule>    logger)
    {
        _bridge  = bridge;
        _config  = config;
        _tracker = tracker;
        _shared  = shared;
        _logger  = logger;

        _onSpawn = OnPlayerSpawnPost;
    }

    public bool Init()
    {
        _bridge.HookManager.PlayerSpawnPost.InstallForward(_onSpawn);
        return true;
    }

    public void Shutdown()
    {
        _bridge.HookManager.PlayerSpawnPost.RemoveForward(_onSpawn);
    }

    private void OnPlayerSpawnPost(IPlayerSpawnForwardParams param)
    {
        if (!_config.Enabled || !_config.RememberLastChoice)
            return;

        var client = param.Client;
        if (client is null || client.IsFakeClient || client.IsHltv)
            return;

        if (!_tracker.IsLive)
            return;

        if (!_shared.ShouldAllow(client!))
            return;

        var cp = _bridge.ClientPreferences;
        if (cp is null || !cp.IsLoaded(client.SteamId))
            return;

        var primaryCookie = cp.GetCookie(client.SteamId, "weaponmenu.primary");
        if (primaryCookie is not null)
        {
            var key = primaryCookie.GetString();
            if (!string.IsNullOrEmpty(key) &&
                Weapons.All.ContainsKey(key) &&
                !IsBlacklisted(key))
            {
                WeaponGiver.Give(client, key);
            }
        }

        var secondaryCookie = cp.GetCookie(client.SteamId, "weaponmenu.secondary");
        if (secondaryCookie is not null)
        {
            var key = secondaryCookie.GetString();
            if (!string.IsNullOrEmpty(key) &&
                Weapons.All.ContainsKey(key) &&
                !IsBlacklisted(key))
            {
                WeaponGiver.Give(client, key);
            }
        }
    }

    private bool IsBlacklisted(string key)
        => _config.Blacklist.Exists(b => string.Equals(b, key, System.StringComparison.OrdinalIgnoreCase));
}
