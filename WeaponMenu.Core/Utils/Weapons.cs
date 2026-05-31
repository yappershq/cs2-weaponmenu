using System.Collections.Generic;

namespace WeaponMenu.Core.Utils;

internal enum WeaponType
{
    Primary,
    Secondary,
}

internal sealed class WeaponEntry
{
    public string     GiveName { get; }
    public WeaponType Type     { get; }
    public int        Slot     { get; }

    public WeaponEntry(string giveName, WeaponType type, int slot)
    {
        GiveName = giveName;
        Type     = type;
        Slot     = slot;
    }
}

internal static class Weapons
{
    public static readonly IReadOnlyDictionary<string, WeaponEntry> All =
        new Dictionary<string, WeaponEntry>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "M4A4",     new WeaponEntry("weapon_m4a1",           WeaponType.Primary,   0) },
            { "M4A1",     new WeaponEntry("weapon_m4a1_silencer",  WeaponType.Primary,   0) },
            { "FAMAS",    new WeaponEntry("weapon_famas",          WeaponType.Primary,   0) },
            { "AUG",      new WeaponEntry("weapon_aug",            WeaponType.Primary,   0) },
            { "AK47",     new WeaponEntry("weapon_ak47",           WeaponType.Primary,   0) },
            { "GALIL",    new WeaponEntry("weapon_galilar",        WeaponType.Primary,   0) },
            { "MP9",      new WeaponEntry("weapon_mp9",            WeaponType.Primary,   0) },
            { "MP7",      new WeaponEntry("weapon_mp7",            WeaponType.Primary,   0) },
            { "MP5SD",    new WeaponEntry("weapon_mp5sd",          WeaponType.Primary,   0) },
            { "UMP45",    new WeaponEntry("weapon_ump45",          WeaponType.Primary,   0) },
            { "P90",      new WeaponEntry("weapon_p90",            WeaponType.Primary,   0) },
            { "BIZON",    new WeaponEntry("weapon_bizon",          WeaponType.Primary,   0) },
            { "MAC10",    new WeaponEntry("weapon_mac10",          WeaponType.Primary,   0) },
            { "XM1014",   new WeaponEntry("weapon_xm1014",         WeaponType.Primary,   0) },
            { "MAG7",     new WeaponEntry("weapon_mag7",           WeaponType.Primary,   0) },
            { "SAWEDOFF", new WeaponEntry("weapon_sawedoff",       WeaponType.Primary,   0) },
            { "NOVA",     new WeaponEntry("weapon_nova",           WeaponType.Primary,   0) },
            { "M249",     new WeaponEntry("weapon_m249",           WeaponType.Primary,   0) },
            { "NEGEV",    new WeaponEntry("weapon_negev",          WeaponType.Primary,   0) },
            { "SG556",    new WeaponEntry("weapon_sg556",          WeaponType.Primary,   0) },
            { "SCAR20",   new WeaponEntry("weapon_scar20",         WeaponType.Primary,   0) },
            { "AWP",      new WeaponEntry("weapon_awp",            WeaponType.Primary,   0) },
            { "SSG08",    new WeaponEntry("weapon_ssg08",          WeaponType.Primary,   0) },
            { "G3SG1",    new WeaponEntry("weapon_g3sg1",          WeaponType.Primary,   0) },
            { "USP",      new WeaponEntry("weapon_usp_silencer",   WeaponType.Secondary, 1) },
            { "P2000",    new WeaponEntry("weapon_hkp2000",        WeaponType.Secondary, 1) },
            { "GLOCK",    new WeaponEntry("weapon_glock",          WeaponType.Secondary, 1) },
            { "DUAL",     new WeaponEntry("weapon_elite",          WeaponType.Secondary, 1) },
            { "P250",     new WeaponEntry("weapon_p250",           WeaponType.Secondary, 1) },
            { "FIVESEVEN",new WeaponEntry("weapon_fiveseven",      WeaponType.Secondary, 1) },
            { "CZ75A",    new WeaponEntry("weapon_cz75a",          WeaponType.Secondary, 1) },
            { "TEC9",     new WeaponEntry("weapon_tec9",           WeaponType.Secondary, 1) },
            { "REVOLVER", new WeaponEntry("weapon_revolver",       WeaponType.Secondary, 1) },
            { "DEAGLE",   new WeaponEntry("weapon_deagle",         WeaponType.Secondary, 1) },
        };
}
