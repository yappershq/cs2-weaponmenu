using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Abstractions;
using WeaponMenu.Core.Configuration;

namespace WeaponMenu.Core;

public class WeaponMenuPlugin : IModSharpModule
{
    private readonly ISharedSystem             _shared;
    private readonly ServiceProvider           _serviceProvider;
    private readonly ILogger<WeaponMenuPlugin> _logger;
    private readonly InterfaceBridge           _bridge;

    public WeaponMenuPlugin(
        ISharedSystem  sharedSystem,
        string         dllPath,
        string         sharpPath,
        Version        version,
        IConfiguration configuration,
        bool           hotReload)
    {
        _shared = sharedSystem;
        var loggerFactory = sharedSystem.GetLoggerFactory();
        _logger = loggerFactory.CreateLogger<WeaponMenuPlugin>();

        var bridge = new InterfaceBridge(
            dllPath,
            sharpPath,
            version,
            sharedSystem,
            this,
            hotReload,
            sharedSystem.GetModSharp().HasCommandLine("-debug"));

        var config = WeaponMenuConfig.Load(sharpPath, _logger);

        var services = new ServiceCollection();

        services.AddSingleton(bridge);
        services.AddSingleton(loggerFactory);
        services.AddSingleton(sharedSystem);
        services.AddSingleton(config);
        services.AddLogging();
        services.AddModuleDi();

        _bridge = bridge;

        _serviceProvider = services.BuildServiceProvider();
    }

    public bool Init()
    {
        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                if (service.Init())
                    continue;

                _logger.LogError("Failed to init {Service}!", service.GetType().FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to init {Service}!", service.GetType().FullName);
            }

            return false;
        }

        return true;
    }

    public void PostInit()
    {
        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                service.OnPostInit(_serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling PostInit for {Service}", service.GetType().FullName);
            }
        }
    }

    public void Shutdown()
    {
        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                service.Shutdown();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling Shutdown for {Service}", service.GetType().FullName);
            }
        }
    }

    public void OnAllModulesLoaded()
    {
        _bridge.ResolveOptionalModules();

        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                service.OnAllModulesLoaded(_serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling OnAllModulesLoaded for {Service}", service.GetType().FullName);
            }
        }
    }

    public void OnLibraryConnected(string name)
    {
        if (name.Equals("ClientPreferences", System.StringComparison.Ordinal))
            _bridge.ResolveClientPreferences();
    }

    string IModSharpModule.DisplayName   => "WeaponMenu";
    string IModSharpModule.DisplayAuthor => "prefix (ported from asapverneri/CS2-Gunsmenu)";
}
