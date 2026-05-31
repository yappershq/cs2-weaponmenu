using System;
using Microsoft.Extensions.Logging;
using Sharp.Modules.AdminManager.Shared;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Modules.MenuManager.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using WeaponMenu.Core.Utils;

namespace WeaponMenu.Core;

internal sealed class InterfaceBridge
{
    internal static InterfaceBridge Instance { get; private set; } = null!;

    internal string          DllPath            { get; }
    internal string          SharpPath          { get; }
    internal Version         Version            { get; }
    internal bool            HotReload          { get; }
    internal bool            Debug              { get; }

    internal IModSharpModule     Module             { get; }
    internal ISharpModuleManager SharpModuleManager { get; }

    internal IConVarManager  ConVarManager  { get; }
    internal IEventManager   EventManager   { get; }
    internal IClientManager  ClientManager  { get; }
    internal IEntityManager  EntityManager  { get; }
    internal IHookManager    HookManager    { get; }
    internal IModSharp       ModSharp       { get; }
    internal ILoggerFactory  LoggerFactory  { get; }

    internal ILocalizerManager? LocalizerManager { get; private set; }
    internal IMenuManager?      MenuManager      { get; private set; }
    internal IAdminManager?     AdminManager     { get; private set; }

    public IGameRules GameRules => ModSharp.GetGameRules();

    public InterfaceBridge(
        string          dllPath,
        string          sharpPath,
        Version         version,
        ISharedSystem   sharedSystem,
        IModSharpModule module,
        bool            hotReload,
        bool            debug)
    {
        DllPath   = dllPath;
        SharpPath = sharpPath;
        Version   = version;
        HotReload = hotReload;
        Debug     = debug;
        Module    = module;

        SharpModuleManager = sharedSystem.GetSharpModuleManager();

        ConVarManager  = sharedSystem.GetConVarManager();
        EventManager   = sharedSystem.GetEventManager();
        ClientManager  = sharedSystem.GetClientManager();
        EntityManager  = sharedSystem.GetEntityManager();
        HookManager    = sharedSystem.GetHookManager();
        ModSharp       = sharedSystem.GetModSharp();
        LoggerFactory  = sharedSystem.GetLoggerFactory();

        Instance = this;
    }

    internal void ResolveOptionalModules()
    {
        if (LocalizerManager is null)
        {
            var iface = SharpModuleManager.GetOptionalSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity);
            if (iface?.Instance is { } lm)
            {
                LocalizerManager = lm;
                lm.LoadLocaleFile("weaponmenu", suppressDuplicationWarnings: true);
            }
        }

        MenuManager  ??= SharpModuleManager.GetOptionalSharpModuleInterface<IMenuManager>(IMenuManager.Identity)?.Instance;
        AdminManager ??= SharpModuleManager.GetOptionalSharpModuleInterface<IAdminManager>(IAdminManager.Identity)?.Instance;
    }

    internal string LocalizeFor(IGameClient client, string key, params object?[] args)
    {
        if (LocalizerManager is null)
            return key;

        try
        {
            return LocalizerManager.For(client).Text(key, args);
        }
        catch (System.Exception)
        {
            return key;
        }
    }

    internal string LocalizeAndColor(IGameClient client, string key, params object?[] args)
        => ChatFormat.ProcessColorCodes(LocalizeFor(client, key, args));
}
