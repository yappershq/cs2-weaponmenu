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

        if (!client.IsInGame)
            return;

        var controller = client.GetPlayerController();
        if (controller is null || !controller.IsValid())
            return;

        var pawn = controller.GetPlayerPawn();
        if (pawn is null || !pawn.IsValid() || !pawn.IsAlive)
            return;

        try
        {
            var gearSlot = entry.Slot == 0 ? GearSlot.Rifle : GearSlot.Pistol;
            var existing = pawn.GetWeaponBySlot(gearSlot);
            if (existing is not null && existing.IsValid())
                pawn.RemovePlayerItem(existing);

            pawn.GiveNamedItem(entry.GiveName);
        }
        catch
        {
        }
    }
}
