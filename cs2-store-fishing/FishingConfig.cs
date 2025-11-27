using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Fishing;

public class FishingConfig : BasePluginConfig
{
    [JsonPropertyName("commands")]
    public List<CommandConfig> Commands { get; set; } = new();

    [JsonPropertyName("regions")]
    public List<RegionConfig> Regions { get; set; } = new();
    
    [JsonPropertyName("boats")]
    public List<BoatConfig> Boats { get; set; } = new();

    [JsonPropertyName("rods")]
    public List<RodConfig> Rods { get; set; } = new();

    [JsonPropertyName("reels")]
    public List<ReelConfig> Reels { get; set; } = new();

    [JsonPropertyName("baits")]
    public List<BaitConfig> Baits { get; set; } = new();

    [JsonPropertyName("fish")]
    public List<FishConfig> Fish { get; set; } = new();
    
    [JsonPropertyName("rarity")]
    public List<RarityConfig> Rarity { get; set; } = new();
}
public class CommandConfig
{
    [JsonPropertyName("fishing_cast_commands")]
    public List<string> CastCommands { get; set; } = ["fish"];
    
    [JsonPropertyName("fishing_region_commands")]
    public List<string> RegionCommands { get; set; } = ["region"];
}

public class RegionConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("flags")]
    public string[] Flags { get; set; } = [];
    
    [JsonPropertyName("allowed_boats")]
    public List<string> AllowedBoats { get; set; } = new();

    [JsonPropertyName("rarity_odds")]
    public Dictionary<string, float> RarityOdds { get; set; } = new();

    [JsonPropertyName("fish_pool")]
    public List<string> FishPool { get; set; } = new();
    
    [JsonPropertyName("distance_from_center")]
    public float DistanceFromCenter { get; set; } = 0f;
}

public class BoatConfig : IPurchasable
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("flags")]
    public string[] Flags { get; set; } = [];

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("speed_modifier")]
    public float Speed { get; set; }
}

public class RodConfig : IPurchasable
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("flags")]
    public string[] Flags { get; set; } = [];

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("max_weight")]
    public float MaxWeight { get; set; }
}

public class ReelConfig : IPurchasable
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("flags")]
    public string[] Flags { get; set; } = [];

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("snap_reduction_percent")]
    public float SnapReductionPercent { get; set; }
}

public class BaitConfig : IPurchasable
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("flags")]
    public string[] Flags { get; set; } = [];

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("bite_chance")]
    public float BiteChance { get; set; }

    [JsonPropertyName("rarity_bias")]
    public Dictionary<string, float> RarityBias { get; set; } = new();
}

public class FishConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("min_weight")]
    public float MinWeight { get; set; }

    [JsonPropertyName("max_weight")]
    public float MaxWeight { get; set; }

    [JsonPropertyName("value_per_lb")]
    public int ValuePerLb { get; set; }
}

public class RarityConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("chance")]
    public float Chance { get; set; }
    
    [JsonPropertyName("value_bonus")]
    public int ValueBonus { get; set; }
}

public interface IPurchasable
{
    string[] Flags { get; }
    int Price { get; }
}