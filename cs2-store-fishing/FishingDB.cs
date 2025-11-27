using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using Microsoft.Data.Sqlite;

namespace Fishing;

public class FishingDB
{
    private readonly string _dbPath;
    public FishingDB(string? dbPath = null)
    {
        if (dbPath == null)
        {
            string pluginDir = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "plugins", "cs2-store-fishing");
            _dbPath = Path.Combine(pluginDir, "fishing.db");
        }
        else
        {
            _dbPath = dbPath;
        }
        
        InitDatabase();
    }
    
    public Dictionary<SteamID, FishingPlayer> Players { get; private set; } = new();
    
    private void InitDatabase()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS fishing_players (
                steamid TEXT PRIMARY KEY,
                rod_id TEXT NOT NULL,
                reel_id TEXT NOT NULL,
                bait_id TEXT NOT NULL,
                region_id TEXT NOT NULL,
                boat_id TEXT NOT NULL,
                autocast BOOL NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }
    
    public FishingPlayer? LoadPlayer(SteamID steamId)
    {
        string steam = steamId.ToString();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT rod_id, reel_id, bait_id, region_id, boat_id, autocast FROM fishing_players WHERE steamid = $steam";
        cmd.Parameters.AddWithValue("$steam", steam);

        using var reader = cmd.ExecuteReader();

        FishingPlayer fp;
        if (reader.Read())
        {
            fp = new FishingPlayer
            {
                RodId = reader.GetString(0),
                ReelId = reader.GetString(1),
                BaitId = reader.GetString(2),
                RegionId = reader.GetString(3),
                BoatId = reader.GetString(4),
                AutoCast = reader.GetBoolean(5)
            };
        }
        else
        {
            return null;
        }

        Players[steamId] = fp;
        return fp;
    }
    
    public void SavePlayer(SteamID steamId, FishingPlayer fp)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO fishing_players(steamid, rod_id, reel_id, bait_id, region_id, boat_id, autocast)
            VALUES ($steam, $rod, $reel, $bait, $region, $boat, $autocast)
            ON CONFLICT(steamid) DO UPDATE SET
                rod_id = excluded.rod_id,
                reel_id = excluded.reel_id,
                bait_id = excluded.bait_id,
                region_id = excluded.region_id,
                boat_id = excluded.boat_id,
                autocast = excluded.autocast;
            """;

        cmd.Parameters.AddWithValue("$steam", steamId.ToString());
        cmd.Parameters.AddWithValue("$rod", fp.RodId);
        cmd.Parameters.AddWithValue("$reel", fp.ReelId);
        cmd.Parameters.AddWithValue("$bait", fp.BaitId);
        cmd.Parameters.AddWithValue("$region", fp.RegionId);
        cmd.Parameters.AddWithValue("$boat", fp.BoatId);
        cmd.Parameters.AddWithValue("$autocast", fp.AutoCast);

        cmd.ExecuteNonQuery();
        
        Players[steamId] = fp;
    }
}