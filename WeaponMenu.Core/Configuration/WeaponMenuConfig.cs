using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace WeaponMenu.Core.Configuration;

internal sealed class WeaponMenuConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("minRound")]
    public int MinRound { get; set; } = 3;

    [JsonPropertyName("countWarmup")]
    public bool CountWarmup { get; set; } = false;

    [JsonPropertyName("permission")]
    public string Permission { get; set; } = string.Empty;

    [JsonPropertyName("weaponCommands")]
    public bool WeaponCommands { get; set; } = true;

    [JsonPropertyName("blacklist")]
    public List<string> Blacklist { get; set; } = new() { "NEGEV" };

    [JsonPropertyName("rememberLastChoice")]
    public bool RememberLastChoice { get; set; } = true;

    /// <summary>Max weapons a player may take per round (1 = one weapon per round). 0 or less = unlimited.</summary>
    [JsonPropertyName("maxPerRound")]
    public int MaxPerRound { get; set; } = 1;

    public static WeaponMenuConfig Load(string sharpPath, ILogger logger)
    {
        var configPath = Path.Combine(sharpPath, "configs", "weaponmenu.json");

        if (!File.Exists(configPath))
        {
            logger.LogWarning("[WeaponMenu] Config not found at {Path}, using defaults", configPath);
            return new WeaponMenuConfig();
        }

        try
        {
            var json   = File.ReadAllText(configPath);
            var result = JsonSerializer.Deserialize<WeaponMenuConfig>(json);
            return result ?? new WeaponMenuConfig();
        }
        catch (System.Exception e)
        {
            logger.LogError(e, "[WeaponMenu] Failed to load config from {Path}", configPath);
            return new WeaponMenuConfig();
        }
    }
}
