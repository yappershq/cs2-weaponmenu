using Sharp.Shared.Objects;

namespace WeaponMenu.Shared;

public interface IWeaponMenuShared
{
    public const string Identity = nameof(IWeaponMenuShared);

    event System.Func<IGameClient, bool>? CanUseFor;
}
