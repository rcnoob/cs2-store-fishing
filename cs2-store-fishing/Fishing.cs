using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using StoreApi;
using CounterStrikeSharp.API.Modules.Entities;

namespace Fishing;

public class Fishing : BasePlugin, IPluginConfig<FishingConfig>
{
    public override string ModuleName => "Store Module [Fishing]";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "rc";
    public static Fishing Instance { get; private set; } = null!;

    private IStoreApi? StoreApi { get; set; }
    private Random Random { get; set; } = new();
    public FishingConfig Config { get; set; }
    private FishingDB _db;

    public override void Load(bool hotReload)
    {
        Instance = this;
        _db = new FishingDB();
        
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterEventHandler<EventPlayerConnectFull>((@event, _) =>
        {
            if (@event.Userid!.IsValid)
            {
                var player = @event.Userid;

                if (player.IsValid && !player.IsBot)
                {
                    SteamID id = (SteamID)player.SteamID;
                    FishingPlayer? fp = _db.LoadPlayer(id);

                    if (fp == null)
                    {
                        fp = CreateNewPlayer();
                        _db.SavePlayer(id, fp);
                    }
                    fp.Player = player;
                }
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid!.IsValid && !@event.Userid.IsBot)
            {
                SteamID id = (SteamID)@event.Userid.SteamID;
                if (_db.Players.TryGetValue(id, out var fp))
                {
                    _db.SavePlayer(id, fp);
                    fp.Player = null;
                }
            }
            return HookResult.Continue;
        });
    }

    public override void Unload(bool hotReload)
    {
        if (_db?.Players != null)
        {
            foreach (var kvp in _db.Players)
            {

                _db.SavePlayer(kvp.Key, kvp.Value);
            }
        }
    }
    
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        StoreApi = IStoreApi.Capability.Get() ?? throw new Exception("StoreApi could not be located.");
        StoreApi.RegisterModules(System.Reflection.Assembly.GetExecutingAssembly());
    }

    private void CreateCommands()
    {
        var cfg = Config.Commands.FirstOrDefault();
        if (cfg == null)
            return;

        foreach (var cmd in cfg.CastCommands)
            AddCommand($"css_{cmd}", "Cast/Reel Line", Command_Cast);
        
        foreach (var cmd in cfg.RegionCommands)
            AddCommand($"css_{cmd}", "Region Options", Command_Region);
        
        AddCommand("css_buyrod", "Buy Rod", Command_BuyRod);
        AddCommand("css_buyreel", "Buy Reel", Command_BuyReel);
        AddCommand("css_buybait", "Buy Bait", Command_BuyBait);
        AddCommand("css_buyboat", "Buy Boat", Command_BuyBoat);
        AddCommand("css_autocast", "Toggle Auto Cast", Command_AutoCast);
    }

    public void OnConfigParsed(FishingConfig config)
    {
        Config = config;
        CreateCommands();
    }
    
    private void Command_AutoCast(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null) return;
    
        SteamID id = (SteamID)player.SteamID;
        if (!_db.Players.TryGetValue(id, out var fp))
            return;

        fp.AutoCast = !fp.AutoCast;
        _db.SavePlayer(id, fp);
    
        string status = fp.AutoCast ? "enabled" : "disabled";
        player.PrintToChat(Localizer["Prefix"] + Localizer["Auto cast toggled", status]);
    }
    
    private void Command_BuyRod(CCSPlayerController player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            SteamID id = (SteamID)player.SteamID;
            if (!_db.Players.TryGetValue(id, out var fp))
                return;
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Shop"]);
            foreach (var rod in Config.Rods)
            {
                bool owned = fp.RodId == rod.Id;
                string status = owned ? "[OWNED]" : $"[${rod.Price}]";
                player.PrintToChat(Localizer["Prefix"] + Localizer["Item list", rod.Id, status]);
            }
            return;
        }
        BuyItem(player, info.GetArg(1).ToLower(), "rod");
    }
    
    private void Command_BuyReel(CCSPlayerController player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            SteamID id = (SteamID)player.SteamID;
            if (!_db.Players.TryGetValue(id, out var fp))
                return;
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Shop"]);
            foreach (var reel in Config.Reels)
            {
                bool owned = fp.ReelId == reel.Id;
                string status = owned ? "[OWNED]" : $"[${reel.Price}]";
                player.PrintToChat(Localizer["Prefix"] + Localizer["Item list", reel.Id, status]);
            }
            return;
        }
        BuyItem(player, info.GetArg(1).ToLower(), "reel");
    }

    private void Command_BuyBait(CCSPlayerController player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            SteamID id = (SteamID)player.SteamID;
            if (!_db.Players.TryGetValue(id, out var fp))
                return;
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Shop"]);
            foreach (var bait in Config.Baits)
            {
                bool owned = fp.BaitId == bait.Id;
                string status = owned ? "[OWNED]" : $"[${bait.Price}]";
                player.PrintToChat(Localizer["Prefix"] + Localizer["Item list", bait.Id, status]);
            }
            return;
        }
        BuyItem(player, info.GetArg(1).ToLower(), "bait");
    }

    private void Command_BuyBoat(CCSPlayerController player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            SteamID id = (SteamID)player.SteamID;
            if (!_db.Players.TryGetValue(id, out var fp))
                return;
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Shop"]);
            foreach (var boat in Config.Boats)
            {
                bool owned = fp.BoatId == boat.Id;
                string status = owned ? "[OWNED]" : $"[${boat.Price}]";
                player.PrintToChat(Localizer["Prefix"] + Localizer["Item list", boat.Id, status]);
            }
            return;
        }
        BuyItem(player, info.GetArg(1).ToLower(), "boat");
    }
    
    private void BuyItem (CCSPlayerController? player, string item, string itemType)
    {
        if (player is null || StoreApi is null)
            return;

        IPurchasable? itemConfig = itemType switch
        {
            "rod" => Config.Rods.FirstOrDefault(r => r.Id == item),
            "reel" => Config.Reels.FirstOrDefault(r => r.Id == item),
            "bait" => Config.Baits.FirstOrDefault(b => b.Id == item),
            "boat" => Config.Boats.FirstOrDefault(b => b.Id == item),
            _ => null
        };

        if (itemConfig == null)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Item not found"]);
            return;
        }

        
        if (itemConfig.Flags.Length > 0 && !AdminManager.PlayerHasPermissions(player, itemConfig.Flags))
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["No permission"]);
            return;
        }

        SteamID id = (SteamID)player.SteamID;
        if (!_db.Players.TryGetValue(id, out var fp))
            return;

        bool ownsItem = itemType switch
        {
            "rod" => fp.RodId == item,
            "reel" => fp.ReelId == item,
            "bait" => fp.BaitId == item,
            "boat" => fp.BoatId == item,
            _ => false
        };
        
        if (ownsItem)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Item already owned"]);
            return;
        }

        int currentCredits = StoreApi.GetPlayerCredits(player);
        
        if (currentCredits < itemConfig.Price)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Not enough credits"]);
            return;
        }
        
        StoreApi.SetPlayerCredits(player, currentCredits - itemConfig.Price);

        switch (itemType)
        {
            case "rod": fp.RodId = item; break;
            case "reel": fp.ReelId = item; break;
            case "bait": fp.BaitId = item; break;
            case "boat": fp.BoatId = item; break;
        }

        _db.SavePlayer(id, fp);

        player.PrintToChat(Localizer["Prefix"] + Localizer["Purchased and equipped", item]);
    }

    private void Command_Cast(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || StoreApi is null) return;
        
        SteamID id = (SteamID)player.SteamID;
        if (!_db.Players.TryGetValue(id, out var fp))
            return;

        if (fp.IsFishing)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Already fishing"]);
            return;
        }

        if (fp.FishIsReady)
        {
            var fish = fp.PendingFish;
            var rod = GetRod(fp.RodId);
            var reel = GetReel(fp.ReelId);
            if (fish is null || rod is null || reel is null) return;

            bool tooHeavy = fish.Fish!.MaxWeight > rod.MaxWeight;
            
            float snapChance = tooHeavy ? 0.90f : 0.10f;
            
            snapChance *= (1f - reel.SnapReductionPercent / 100f);
            
            float roll = (float)Random.NextDouble();
            if (roll < snapChance)
            {
                player.PrintToChat(Localizer["Prefix"] + Localizer["Line snapped", fish.Rarity!.Id, fish.Fish.Id, fish.TotalValue]);
                fp.FishIsReady = false;
                fp.PendingFish = null;
                
                if (fp.AutoCast)
                {
                    fp.IsFishing = true;
                    fp.TimeUntilNextBiteRoll = 10f;
                    player.PrintToChat(Localizer["Prefix"] + Localizer["Line cast"]);
                }
                return;
            }
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Caught fish", fish.Rarity!.Id, fish.Fish.Id, fish.TotalValue]);
            StoreApi.GivePlayerCredits(player, fish.TotalValue);
            fp.IsFishing = false;
            fp.FishIsReady = false;
            fp.PendingFish = null;
            
            if (fp.AutoCast)
            {
                fp.IsFishing = true;
                fp.TimeUntilNextBiteRoll = 10f;
                player.PrintToChat(Localizer["Prefix"] + Localizer["Line cast"]);
            }
            return;
        }
        
        fp.IsFishing = true;
        fp.FishIsReady = false;
        fp.TimeUntilNextBiteRoll = 10f;

        player.PrintToChat(Localizer["Prefix"] + Localizer["Line cast"]);
    }
    
    private void Command_Region(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || StoreApi is null) 
            return;
        
        if (info.ArgCount < 2)
        {
            SteamID id = (SteamID)player.SteamID;
            if (!_db.Players.TryGetValue(id, out var fp))
                return;
            
            player.PrintToChat(Localizer["Prefix"] + Localizer["Region"]);
            foreach (var region in Config.Regions)
            {
                bool currentRegion = fp.RegionId == region.Id;
                string status = currentRegion ? " - [HERE]" : "";
                player.PrintToChat(Localizer["Prefix"] + Localizer["Region list", region.Id, status]);
            }
            return;
        }

        string regionName = info.GetArg(1).ToLower();

        var targetRegion = Config.Regions.FirstOrDefault(r => r.Id.ToLower() == regionName);
        if (targetRegion is null)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Region not found"]);
            return;
        }
        
        TravelToRegion(player, regionName);
    }
    
    public void OnTick()
    {
        foreach (var kvp in _db.Players)
        {
            SteamID steam = kvp.Key;
            var fp = kvp.Value;
            if (fp.Player is null) continue;
            
            // fishing
            if (fp.IsFishing)
            {
                fp.TimeUntilNextBiteRoll -= Server.FrameTime;

                if (fp.TimeUntilNextBiteRoll <= 0f)
                {
                    fp.TimeUntilNextBiteRoll = 10f;

                    var bait = GetBait(fp.BaitId);
                    if (bait != null)
                    {
                        float roll = (float)Random.NextDouble();

                        if (roll <= bait.BiteChance)
                        {
                            fp.IsFishing = false;
                            fp.FishIsReady = true;
                            fp.PendingFish = GenerateRandomFish(fp);
                            
                            fp.Player.PrintToChat(Localizer["Prefix"] + Localizer["Fish bite"]);
                        }
                        else
                        {
                            fp.Player.PrintToChat(Localizer["Prefix"] + Localizer["Waiting for bite"]);
                        }
                    }
                }
            }

            // traveling
            if (fp.TravelTimeLeft > 0f)
            {
                fp.TravelTimeLeft -= Server.FrameTime;

                if (fp.TravelTimeLeft <= 0f)
                {
                    fp.TravelTimeLeft = 0f;

                    fp.RegionId = fp.Destination;
                    fp.Destination = "";
                    
                    fp.Player.PrintToChat(Localizer["Prefix"] + Localizer["Arrived", fp.RegionId]);

                    _db.SavePlayer(steam, fp);
                }
            }
        }
    }
    
    private void TravelToRegion(CCSPlayerController player, string region)
    {
        SteamID id = (SteamID)player.SteamID;
        if (!_db.Players.TryGetValue(id, out var fp)) return;

        var currentRegion = Config.Regions.FirstOrDefault(r => r.Id == fp.RegionId);
        var targetRegion = Config.Regions.FirstOrDefault(r => r.Id == region);
        if (targetRegion == null) return;

        if (fp.RegionId == targetRegion.Id)
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["You are already here"]);
            return;
        }

        if (targetRegion.Flags.Length > 0 && !AdminManager.PlayerHasPermissions(player, targetRegion.Flags))
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["No permission"]);
            return;
        }

        if (!targetRegion.AllowedBoats.Contains(fp.BoatId))
        {
            player.PrintToChat(Localizer["Prefix"] + Localizer["Boat not allowed"]);
            return;
        }

        var boat = Config.Boats.FirstOrDefault(b => b.Id == fp.BoatId);
        float speed = boat?.Speed ?? 1f;
        
        float distance = Math.Abs(targetRegion.DistanceFromCenter - (currentRegion?.DistanceFromCenter ?? 0f));

        fp.TravelTimeLeft = distance / speed;
        fp.Destination = targetRegion.Id;

        player.PrintToChat(Localizer["Prefix"] + Localizer["Traveling", targetRegion.Id, Math.Round(fp.TravelTimeLeft)]);
    }
    
    private readonly Random _rng = new();

    private RarityConfig RollRarityWithModifiers(FishingPlayer fp)
    {
        var rarities = Config.Rarity;

        var region = Config.Regions.FirstOrDefault(r => r.Id == fp.RegionId);
        var regionMod = region?.RarityOdds ?? new Dictionary<string, float>();

        var bait = GetBait(fp.BaitId);
        var baitMod = bait?.RarityBias ?? new Dictionary<string, float>();
        
        Dictionary<RarityConfig, float> weights = new();
        foreach (var r in rarities)
        {
            float weight = r.Chance;

            if (regionMod.TryGetValue(r.Id, out float rm)) weight *= rm;
            if (baitMod.TryGetValue(r.Id, out float bm)) weight *= bm;

            weight = Math.Max(0f, weight);
            weights[r] = weight;
        }
        
        float total = weights.Values.Sum();
        float roll = (float)(_rng.NextDouble() * total);
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return rarities[0];
    }
    
    public CaughtFish GenerateRandomFish(FishingPlayer fp)
    {
        if (fp.RegionId == null) 
            throw new Exception("Player not in a region!");

        var region = Config.Regions.FirstOrDefault(r => r.Id == fp.RegionId);

        var rarity = RollRarityWithModifiers(fp);

        List<FishConfig> pool = (region?.FishPool.Count > 0)
            ? Config.Fish.Where(f => region.FishPool.Contains(f.Id)).ToList()
            : Config.Fish;

        if (pool.Count == 0) 
            throw new Exception("No fish available in this region!");

        var fish = pool[_rng.Next(pool.Count)];

        double weight = fish.MinWeight + _rng.NextDouble() * (fish.MaxWeight - fish.MinWeight);

        double rawValue = weight * fish.ValuePerLb * rarity.ValueBonus;

        int totalValue = Math.Max(1, (int)Math.Round(rawValue));
        
        return new CaughtFish
        {
            Fish = fish,
            Rarity = rarity,
            TotalValue = totalValue
        };
    }
    
    private RodConfig? GetRod(string id) =>
        Config.Rods.FirstOrDefault(r => r.Id == id);

    private ReelConfig? GetReel(string id) =>
        Config.Reels.FirstOrDefault(r => r.Id == id);

    private BaitConfig? GetBait(string id) =>
        Config.Baits.FirstOrDefault(b => b.Id == id);
    
    private FishingPlayer CreateNewPlayer()
    {
        return new FishingPlayer
        {
            RodId = Config.Rods.MinBy(r => r.Price)?.Id ?? "basic_rod",
            ReelId = Config.Reels.MinBy(r => r.Price)?.Id ?? "basic_reel",
            BaitId = Config.Baits.MinBy(b => b.Price)?.Id ?? "none",
            BoatId = Config.Boats.MinBy(b => b.Price)?.Id ?? "raft",
            RegionId = Config.Regions.FirstOrDefault()?.Id ?? "shallow_waters"
        };
    }
}

public class CaughtFish
{
    public FishConfig? Fish { get; set; }
    public RarityConfig? Rarity { get; set; }
    public int TotalValue { get; set; }
}