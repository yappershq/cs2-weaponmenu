using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;

namespace WeaponMenu.Core.Utils;

internal static class WeaponGiver
{
    internal static void Give(IGameClient client, string weaponKey)
    {
        if (!Weapons.All.TryGetValue(weaponKey, out var entry))
            return;

        var controller = client.GetPlayerController();
        var pawn       = controller?.GetPlayerPawn();
        if (pawn is null)
            return;

        var gearSlot = entry.Slot == 0 ? GearSlot.Rifle : GearSlot.Pistol;
        var existing = pawn.GetWeaponBySlot(gearSlot);
        if (existing is not null)
            pawn.RemovePlayerItem(existing);

        pawn.GiveNamedItem(entry.GiveName);
    }
}
