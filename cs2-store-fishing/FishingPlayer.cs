using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Fishing;

public class FishingPlayer
{
    public string RodId = "basic_rod";
    public string ReelId = "basic_reel";
    public string BaitId = "none";
    public string RegionId = "coastal_waters";
    public string BoatId = "raft";
    
    public bool IsFishing = false;
    public bool FishIsReady = false;
    public float TimeUntilNextBiteRoll = 0f;
    public float TravelTimeLeft = 0f;
    public string Destination = "";
    public CaughtFish? PendingFish = null;
    public bool AutoCast = false;
    
    [JsonIgnore] 
    public CCSPlayerController? Player { get; set; } = null;
}