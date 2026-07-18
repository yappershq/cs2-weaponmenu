using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sharp.Modules.MenuManager.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using WeaponMenu.Core.Configuration;
using WeaponMenu.Core.Utils;

namespace WeaponMenu.Core.Modules;

internal sealed class WeaponMenuModule : IModule
{
    private readonly InterfaceBridge           _bridge;
    private readonly WeaponMenuConfig          _config;
    private readonly RoundTrackerModule        _tracker;
    private readonly SharedInterfaceModule     _shared;
    private readonly ILogger<WeaponMenuModule> _logger;

    private readonly IClientManager.DelegateClientCommand _onGuns;
    private readonly IClientManager.DelegateClientCommand _onPrimary;
    private readonly IClientManager.DelegateClientCommand _onSecondary;

    private readonly List<(string cmd, IClientManager.DelegateClientCommand cb)> _weaponCallbacks = new();

    // Per-round give budget: last live-round a slot took a weapon + how many it took that round.
    private readonly int[] _givenRound = new int[64];
    private readonly int[] _givenCount = new int[64];

    public WeaponMenuModule(
        InterfaceBridge           bridge,
        WeaponMenuConfig          config,
        RoundTrackerModule        tracker,
        SharedInterfaceModule     shared,
        ILogger<WeaponMenuModule> logger)
    {
        _bridge  = bridge;
        _config  = config;
        _tracker = tracker;
        _shared  = shared;
        _logger  = logger;

        _onGuns      = OnGunsCommand;
        _onPrimary   = OnPrimaryCommand;
        _onSecondary = OnSecondaryCommand;
    }

    public bool Init()
    {
        if (!_config.Enabled)
            return true;

        var cm = _bridge.ClientManager;
        cm.InstallCommandCallback("guns",      _onGuns);
        cm.InstallCommandCallback("primary",   _onPrimary);
        cm.InstallCommandCallback("secondary", _onSecondary);

        if (_config.WeaponCommands)
        {
            foreach (var key in Weapons.All.Keys)
            {
                if (IsBlacklisted(key))
                    continue;

                var capturedKey = key;
                var cmd         = key.ToLowerInvariant();

                IClientManager.DelegateClientCommand cb = (client, _) => OnWeaponCommand(client, capturedKey);
                _weaponCallbacks.Add((cmd, cb));
                cm.InstallCommandCallback(cmd, cb);
            }
        }

        _logger.LogInformation("[WeaponMenu] Commands installed");
        return true;
    }

    public void Shutdown()
    {
        var cm = _bridge.ClientManager;
        cm.RemoveCommandCallback("guns",      _onGuns);
        cm.RemoveCommandCallback("primary",   _onPrimary);
        cm.RemoveCommandCallback("secondary", _onSecondary);

        foreach (var (cmd, cb) in _weaponCallbacks)
            cm.RemoveCommandCallback(cmd, cb);

        _weaponCallbacks.Clear();
    }

    private ECommandAction OnGunsCommand(IGameClient client, StringCommand command)
    {
        if (client.IsFakeClient)
            return ECommandAction.Skipped;

        if (!CheckGates(client))
            return ECommandAction.Handled;

        var menu = BuildWeaponMenu(null);
        if (menu is not null)
            _bridge.MenuManager!.DisplayMenu(client, menu);

        return ECommandAction.Handled;
    }

    private ECommandAction OnPrimaryCommand(IGameClient client, StringCommand command)
    {
        if (client.IsFakeClient)
            return ECommandAction.Skipped;

        if (!CheckGates(client))
            return ECommandAction.Handled;

        var menu = BuildWeaponMenu(WeaponType.Primary);
        if (menu is not null)
            _bridge.MenuManager!.DisplayMenu(client, menu);

        return ECommandAction.Handled;
    }

    private ECommandAction OnSecondaryCommand(IGameClient client, StringCommand command)
    {
        if (client.IsFakeClient)
            return ECommandAction.Skipped;

        if (!CheckGates(client))
            return ECommandAction.Handled;

        var menu = BuildWeaponMenu(WeaponType.Secondary);
        if (menu is not null)
            _bridge.MenuManager!.DisplayMenu(client, menu);

        return ECommandAction.Handled;
    }

    private ECommandAction OnWeaponCommand(IGameClient client, string weaponKey)
    {
        if (client.IsFakeClient)
            return ECommandAction.Skipped;

        if (!CheckGates(client))
            return ECommandAction.Handled;

        if (!TryConsumeGive(client))
            return ECommandAction.Handled;

        WeaponGiver.Give(client, weaponKey);
        SaveWeaponCookie(client, weaponKey);
        client.Print(HudPrintChannel.Chat,
            _bridge.LocalizeAndColor(client, "weaponmenu.weapon_given", weaponKey));
        return ECommandAction.Handled;
    }

    /// <summary>
    /// Enforce the per-round weapon budget (config MaxPerRound; default 1 = one weapon/round).
    /// Resets automatically when the live round number changes. 0 or less = unlimited.
    /// </summary>
    private bool TryConsumeGive(IGameClient client)
    {
        if (_config.MaxPerRound <= 0)
            return true;

        var slot = (int) (byte) client.Slot;
        if ((uint) slot >= 64)
            return true;

        var round = _tracker.CurrentLiveRound;
        if (_givenRound[slot] != round)
        {
            _givenRound[slot] = round;
            _givenCount[slot] = 0;
        }

        if (_givenCount[slot] >= _config.MaxPerRound)
        {
            client.Print(HudPrintChannel.Chat,
                _bridge.LocalizeAndColor(client, "weaponmenu.round_limit", _config.MaxPerRound));
            return false;
        }

        _givenCount[slot]++;
        return true;
    }

    private void SaveWeaponCookie(IGameClient client, string weaponKey)
    {
        if (!_config.RememberLastChoice)
            return;

        var cp = _bridge.ClientPreferences;
        if (cp is null || !cp.IsLoaded(client.SteamId))
            return;

        if (!Weapons.All.TryGetValue(weaponKey, out var entry))
            return;

        var cookieKey = entry.Type == WeaponType.Primary ? "weaponmenu.primary" : "weaponmenu.secondary";
        cp.SetCookie(client.SteamId, cookieKey, weaponKey);
    }

    private bool CheckGates(IGameClient client)
    {
        var pawn = client.GetPlayerController()?.GetPlayerPawn();
        if (pawn is null || !pawn.IsAlive)
        {
            client.Print(HudPrintChannel.Chat,
                _bridge.LocalizeAndColor(client, "weaponmenu.not_alive"));
            return false;
        }

        if (!string.IsNullOrEmpty(_config.Permission))
        {
            var am = _bridge.AdminManager;
            if (am?.GetAdmin(client.SteamId) is not { } admin || !admin.HasPermission(_config.Permission))
            {
                client.Print(HudPrintChannel.Chat,
                    _bridge.LocalizeAndColor(client, "weaponmenu.no_permission"));
                return false;
            }
        }

        if (!_shared.ShouldAllow(client))
        {
            client.Print(HudPrintChannel.Chat,
                _bridge.LocalizeAndColor(client, "weaponmenu.not_allowed"));
            return false;
        }

        if (_tracker.CurrentLiveRound < _config.MinRound)
        {
            client.Print(HudPrintChannel.Chat,
                _bridge.LocalizeAndColor(client, "weaponmenu.round_locked", _config.MinRound));
            return false;
        }

        return true;
    }

    private Menu? BuildWeaponMenu(WeaponType? filter)
    {
        if (_bridge.MenuManager is null)
            return null;

        var titleKey = filter switch
        {
            WeaponType.Primary   => "weaponmenu.menu.primary",
            WeaponType.Secondary => "weaponmenu.menu.secondary",
            _                    => "weaponmenu.menu.title",
        };

        var menu = new Menu();
        menu.SetTitle(c => _bridge.LocalizeFor(c, titleKey));

        foreach (var (key, entry) in Weapons.All)
        {
            if (IsBlacklisted(key))
                continue;

            if (filter.HasValue && entry.Type != filter.Value)
                continue;

            var capturedKey = key;
            menu.AddItem(key, ctrl =>
            {
                var c = ctrl.Client;
                if (!c.IsInGame)
                    return;
                var cbPawn = c.GetPlayerController()?.GetPlayerPawn();
                if (cbPawn is null || !cbPawn.IsAlive)
                    return;
                WeaponGiver.Give(c, capturedKey);
                SaveWeaponCookie(c, capturedKey);
                c.Print(HudPrintChannel.Chat,
                    _bridge.LocalizeAndColor(c, "weaponmenu.weapon_given", capturedKey));
                ctrl.Exit();
            });
        }

        if (filter is null && _config.RememberLastChoice)
        {
            menu.AddItem(
                c => _bridge.LocalizeFor(c, "weaponmenu.menu.forget"),
                ctrl =>
                {
                    var c = ctrl.Client;
                    if (!c.IsInGame)
                        return;
                    var cp = _bridge.ClientPreferences;
                    if (cp is not null && cp.IsLoaded(c.SteamId))
                    {
                        cp.SetCookie(c.SteamId, "weaponmenu.primary",   string.Empty);
                        cp.SetCookie(c.SteamId, "weaponmenu.secondary", string.Empty);
                    }
                    c.Print(HudPrintChannel.Chat,
                        _bridge.LocalizeAndColor(c, "weaponmenu.preference.cleared"));
                    ctrl.Exit();
                });
        }

        return menu;
    }

    private bool IsBlacklisted(string key)
        => _config.Blacklist.Any(b => string.Equals(b, key, System.StringComparison.OrdinalIgnoreCase));
}
