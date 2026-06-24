<div align="center">
  <h1><strong>WeaponMenu</strong></h1>
  <p>Let players pick their primary/secondary loadout from a menu (or chat command) each round — for CS2 servers running ModSharp.</p>
</div>

<p align="center">
  <a href="https://github.com/Kxnrl/modsharp-public"><img src="https://img.shields.io/badge/framework-ModSharp-5865F2?logo=github" alt="ModSharp"></a>
  <img src="https://img.shields.io/badge/game-CS2-orange" alt="CS2">
  <img src="https://img.shields.io/github/stars/yappershq/cs2-weaponmenu?style=flat&logo=github" alt="Stars">
</p>

---

WeaponMenu gives players a `!guns` menu to choose a weapon, optionally exposes one chat/console command per weapon (`!ak47`, `!awp`, …), remembers the last choice and auto-gives it on spawn, and gates everything behind a configurable starting round and per-round budget. It is a ModSharp port of [asapverneri/CS2-Gunsmenu](https://github.com/asapverneri/CS2-Gunsmenu).

## 🚀 Install

Copy the build output into your ModSharp install (`<sharp>` = your `sharp` directory):

| From | To |
|------|----|
| `.build/modules/WeaponMenu.Core/` | `<sharp>/modules/WeaponMenu.Core/` |
| `.build/shared/WeaponMenu.Shared/` | `<sharp>/shared/WeaponMenu.Shared/` |
| `.assets/configs/weaponmenu.json` | `<sharp>/configs/weaponmenu.json` |
| `.assets/locales/weaponmenu.json` | `<sharp>/locales/weaponmenu.json` |

Restart the server (or change map) to load. Requires the ModSharp **MenuManager**, **LocalizerManager**, **AdminManager**, and **ClientPreferences** modules.

## ⌨️ Commands

All commands work from both chat (`!guns`) and console (`guns`).

| Command | Description |
|---------|-------------|
| `guns` | Open the full weapon menu (primary + secondary) |
| `primary` | Open the primary-weapons menu |
| `secondary` | Open the secondary-weapons menu |
| `<weapon>` | Directly give a weapon, e.g. `ak47`, `awp`, `deagle` — one command per supported weapon (only when `weaponCommands` is enabled) |

Commands are blocked unless the player is alive, has the configured permission (if set), and the live round is at least `minRound`.

## ⚙️ Configuration

`configs/weaponmenu.json`:

| Setting | Default | Meaning |
|---------|---------|---------|
| `enabled` | `true` | Master switch for the plugin |
| `minRound` | `3` | First live round the menu/commands become usable |
| `countWarmup` | `false` | Whether warmup rounds count toward `minRound` |
| `permission` | `""` | AdminManager permission required to use the menu (empty = everyone) |
| `weaponCommands` | `true` | Register a per-weapon chat/console command for each weapon |
| `blacklist` | `["NEGEV"]` | Weapon keys hidden from the menu and commands |
| `rememberLastChoice` | `true` | Save the last primary/secondary pick and auto-give it on spawn |
| `maxPerRound` | `1` | Max weapons a player may take per round (`0` or less = unlimited) |

## 🔧 How it works

Commands are installed through ModSharp's client-command system, so each one responds to both chat and console. Picking a weapon from the menu (or via a direct command) calls a weapon giver and, when `rememberLastChoice` is on, stores the choice in ClientPreferences cookies. On spawn the auto-give module reads those cookies and re-equips the saved loadout. A round tracker counts live rounds (optionally including warmup) so the `minRound` gate and per-round budget reset correctly across round transitions.

## 🧩 Public API

Other plugins can veto access per client by subscribing to `IWeaponMenuShared` (resolve in `OnAllModulesLoaded`):

```csharp
var api = sharpModuleManager
    .GetOptionalSharpModuleInterface<IWeaponMenuShared>(IWeaponMenuShared.Identity)?.Instance;

// Return false to block a client from using the menu/commands.
api.CanUseFor += client => client.Team == CStrikeTeam.CT;
```

## 📦 Build

```bash
dotnet build -c Release
```

Outputs `.build/modules/WeaponMenu.Core/WeaponMenu.Core.dll` and `.build/shared/WeaponMenu.Shared/WeaponMenu.Shared.dll`.

## 🙏 Credits

Port of [asapverneri/CS2-Gunsmenu](https://github.com/asapverneri/CS2-Gunsmenu).

---

<div align="center">
  <p>Made with ❤️ by <a href="https://github.com/yappershq">yappershq</a></p>
  <p>⭐ Star this repo if you find it useful!</p>
</div>
