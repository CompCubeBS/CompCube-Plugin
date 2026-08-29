using CompCube.UI.BSML.Components;
using Newtonsoft.Json;

namespace CompCube.Models;

/** Contains the small amount of data the game UI needs independently of the server's wire format. */
[method: JsonConstructor]
public sealed class Badge(string name, string colorCode, bool bold)
{
    public string Name { get; } = name;
    public string ColorCode { get; } = colorCode;
    public bool Bold { get; } = bold;
}

/** Describes the competitive division shown by the in-game leaderboard. */
public sealed class DivisionInfo
{
    [JsonConstructor]
    public DivisionInfo(string division, int subDivision, string colorCode, bool glow)
    {
        Division = division;
        SubDivision = subDivision;
        Color = colorCode;
        Glow = glow;
    }

    public DivisionInfo(DivisionName division, int subDivision, string colorCode, bool glow)
        : this(division.ToString(), subDivision, colorCode, glow) { }

    public string Division { get; }
    public int SubDivision { get; }
    public string Color { get; }
    public bool Glow { get; }

    public enum DivisionName
    {
        Iron,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Iridescent,
        Luminal,
        Superluminal,
    }
}

/** Represents one user in the plugin without mirroring the complete database record. */
public sealed class UserInfo
{
    [JsonConstructor]
    public UserInfo(
        string username,
        string userId,
        int mmr,
        Badge? badge,
        long rank,
        string? discordId,
        bool banned,
        int wins,
        int totalGames,
        int winstreak,
        int highestWinstreak,
        string? profilePictureLink = null)
    {
        Username = username;
        UserId = userId;
        Mmr = mmr;
        Badge = badge;
        Rank = rank;
        DiscordId = discordId;
        Banned = banned;
        Wins = wins;
        TotalGames = totalGames;
        Winstreak = winstreak;
        HighestWinstreak = highestWinstreak;
        ProfilePictureLink = profilePictureLink
            ?? $"https://cdn.scoresaber.com/avatars/{(userId.Length == 17 ? $"{userId}.jpg" : "oculus.png")}";
    }

    public string Username { get; }
    public string UserId { get; }
    public string ProfilePictureLink { get; }
    public int Mmr { get; }
    public Badge? Badge { get; }
    public long Rank { get; }
    public string? DiscordId { get; }
    public bool Banned { get; }
    public int Wins { get; }
    public int TotalGames { get; }
    public int Winstreak { get; }
    public int HighestWinstreak { get; }
}

/** Describes one map entry delivered as part of the current match. */
public sealed class VotingMap : IEquatable<VotingMap>
{
    [JsonConstructor]
    public VotingMap(
        string hash,
        DifficultyType difficulty,
        Category category,
        string guid = "",
        string characteristic = "Standard",
        string[]? modifiers = null,
        int durationSeconds = 0,
        int maxScore = 0)
    {
        Hash = hash;
        Difficulty = difficulty;
        MapCategory = category;
        Guid = guid;
        Characteristic = characteristic;
        Modifiers = modifiers ?? [];
        DurationSeconds = durationSeconds;
        MaxScore = maxScore;
    }

    public string Hash { get; }
    public string Guid { get; }
    public string Characteristic { get; }
    public string[] Modifiers { get; }
    public int DurationSeconds { get; }
    public int MaxScore { get; }
    public DifficultyType Difficulty { get; }
    public Category MapCategory { get; }

    public bool Equals(VotingMap? other) => other != null && Hash == other.Hash && Difficulty == other.Difficulty;
    public override bool Equals(object? value) => value is VotingMap other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            return (Hash.GetHashCode() * 397) ^ (int)Difficulty;
        }
    }
    public static bool operator ==(VotingMap? left, VotingMap? right) => Equals(left, right);
    public static bool operator !=(VotingMap? left, VotingMap? right) => !Equals(left, right);

    public enum Category
    {
        Accuracy,
        MidSpeed,
        Tech,
        Speed,
        Extreme,
        Special,
    }

    public enum DifficultyType
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus,
    }
}

/** Holds the score fields displayed after a map has finished. */
public sealed class Score(int points, float accuracy, bool proMode, int misses, bool fullCombo)
{
    public static Score Empty => new(0, 0f, false, 0, true);
    public int Points { get; } = points;
    public float Accuracy { get; } = accuracy;
    public bool ProMode { get; } = proMode;
    public int Misses { get; } = misses;
    public bool FullCombo { get; } = fullCombo;
}

public sealed class MatchScore(UserInfo user, Score? score)
{
    public UserInfo User { get; } = user;
    public Score? Score { get; } = score;
}

public sealed class EventData(string eventName, string displayName, string description, string eventOwner, ulong eventOwnerId, bool availableToJoin)
{
    public string EventName { get; } = eventName;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
    public string EventOwner { get; } = eventOwner;
    public ulong EventOwnerId { get; } = eventOwnerId;
    public bool AvailableToJoin { get; } = availableToJoin;
}

public sealed class ServerStatus(string[] allowedGameVersions, string[] allowedModVersions, ServerState state)
{
    public string[] AllowedGameVersions { get; } = allowedGameVersions;
    public string[] AllowedModVersions { get; } = allowedModVersions;
    public ServerState State { get; } = state;
}

[method: JsonConstructor]
public sealed class Queue(string guid, string slug, string name, string poolGuid, bool competitive, bool enabled)
{
    [JsonProperty("guid")]
    public readonly string Guid = guid;
    
    [JsonProperty("slug")]
    public readonly string Slug = slug;
    
    [JsonProperty("name")]
    public readonly string Name = name;
    
    [JsonProperty("poolGuid")]
    public readonly string PoolGuid = poolGuid;
    
    [JsonProperty("competitive")]
    public readonly bool Competitive = competitive;
    
    [JsonProperty("enabled")]
    public readonly bool Enabled = enabled;

    public QueueOptionTab ToQueueOptionTab() => new(Name, Slug);
}

public enum ServerState
{
    Online,
    Maintenance,
}
