namespace WeaponMenu.Core;

internal interface IModule
{
    bool Init();

    void OnPostInit(System.IServiceProvider provider)
    {
    }

    void OnAllModulesLoaded(System.IServiceProvider provider)
    {
    }

    void Shutdown()
    {
    }
}
