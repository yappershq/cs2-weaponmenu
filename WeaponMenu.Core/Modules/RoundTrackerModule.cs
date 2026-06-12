using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using WeaponMenu.Core.Configuration;

namespace WeaponMenu.Core.Modules;

internal sealed class RoundTrackerModule : IModule, IEventListener, IGameListener
{
    private readonly InterfaceBridge  _bridge;
    private readonly WeaponMenuConfig _config;

    private int  _liveRound;

    public int  CurrentLiveRound => _liveRound;
    public bool IsLive           => _liveRound >= _config.MinRound && !IsWarmup();

    private bool IsWarmup()
    {
        try   { return _bridge.GameRules.IsWarmupPeriod; }
        catch { return false; }
    }

    int IEventListener.ListenerPriority => 0;
    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IGameListener.ListenerPriority  => 0;
    int IGameListener.ListenerVersion   => IGameListener.ApiVersion;

    public RoundTrackerModule(InterfaceBridge bridge, WeaponMenuConfig config)
    {
        _bridge = bridge;
        _config = config;
    }

    public bool Init()
    {
        _bridge.ModSharp.InstallGameListener(this);
        _bridge.EventManager.InstallEventListener(this);
        _bridge.EventManager.HookEvent("round_start");
        _bridge.EventManager.HookEvent("announce_phase_end");
        return true;
    }

    public void Shutdown()
    {
        _bridge.ModSharp.RemoveGameListener(this);
        _bridge.EventManager.RemoveEventListener(this);
    }

    void IGameListener.OnGameActivate()
    {
        _liveRound = 0;
    }

    void IGameListener.OnGameInit()
    {
        _liveRound = 0;
    }

    void IEventListener.FireGameEvent(IGameEvent @event)
    {
        if (@event.Name == "announce_phase_end")
        {
            // Halftime / overtime side swap: economy resets, so the
            // early-round weapon restriction starts over as well.
            _liveRound = 0;
            return;
        }

        if (@event.Name != "round_start")
            return;

        bool isWarmup;
        try
        {
            isWarmup = _bridge.GameRules.IsWarmupPeriod;
        }
        catch
        {
            isWarmup = false;
        }

        if (isWarmup && !_config.CountWarmup)
            return;

        _liveRound++;
    }
}
