using Microsoft.Extensions.Logging;
using Sharp.Shared.Enums;
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

        var primaryKey   = cp.GetCookie(client.SteamId, "weaponmenu.primary")?.GetString();
        var secondaryKey = cp.GetCookie(client.SteamId, "weaponmenu.secondary")?.GetString();

        if (string.IsNullOrEmpty(primaryKey) && string.IsNullOrEmpty(secondaryKey))
            return;

        var steamId = client.SteamId;

        _bridge.ModSharp.PushTimer(() =>
        {
            var c = _bridge.ClientManager.GetGameClient(steamId);
            if (c is null || !c.IsInGame)
                return;

            if (!string.IsNullOrEmpty(primaryKey) &&
                Weapons.All.ContainsKey(primaryKey) &&
                !IsBlacklisted(primaryKey))
            {
                WeaponGiver.Give(c, primaryKey);
            }

            if (!string.IsNullOrEmpty(secondaryKey) &&
                Weapons.All.ContainsKey(secondaryKey) &&
                !IsBlacklisted(secondaryKey))
            {
                WeaponGiver.Give(c, secondaryKey);
            }
        }, 0.0001, GameTimerFlags.StopOnMapEnd);
    }

    private bool IsBlacklisted(string key)
        => _config.Blacklist.Exists(b => string.Equals(b, key, System.StringComparison.OrdinalIgnoreCase));
}
