using ProtoBuf;

namespace CompCube.Networking.Replay;

// These fields intentionally mirror scoresaber.live.v1/replay_stream.proto so ChroViewer can consume
// the exact same binary stream it already accepts from TournamentAssistant.
[ProtoContract]
public sealed class ReplayStreamPacket
{
    [ProtoMember(1)] public string StreamId { get; set; } = string.Empty;
    [ProtoMember(3)] public string PlayerId { get; set; } = string.Empty;
    [ProtoMember(4)] public string MatchId { get; set; } = string.Empty;
    [ProtoMember(10)] public ReplayStreamStart? Start { get; set; }
    [ProtoMember(11)] public ReplayChunk? Chunk { get; set; }
    [ProtoMember(13)] public ReplayStreamEnd? End { get; set; }
}

[ProtoContract]
public sealed class ReplayStreamStart
{
    [ProtoMember(1)] public uint ProtocolVersion { get; set; } = 1;
    [ProtoMember(2)] public PlayerIdentity? Player { get; set; }
    [ProtoMember(3)] public BeatmapIdentity? Beatmap { get; set; }
    [ProtoMember(9)] public long ClientStartTimeUnixMs { get; set; }
    [ProtoMember(11)] public string GameSessionId { get; set; } = string.Empty;
    [ProtoMember(13)] public StreamReplayMetadata? ReplayMetadata { get; set; }
}

[ProtoContract]
public sealed class PlayerIdentity
{
    [ProtoMember(1)] public string PlayerId { get; set; } = string.Empty;
    [ProtoMember(3)] public string GameVersion { get; set; } = string.Empty;
    [ProtoMember(4)] public string ClientVersion { get; set; } = string.Empty;
}

[ProtoContract]
public sealed class BeatmapIdentity
{
    [ProtoMember(1)] public string MapHash { get; set; } = string.Empty;
    [ProtoMember(2)] public string LevelId { get; set; } = string.Empty;
    [ProtoMember(3)] public int Difficulty { get; set; }
    [ProtoMember(4)] public string DifficultyName { get; set; } = string.Empty;
    [ProtoMember(5)] public string Characteristic { get; set; } = "Standard";
    [ProtoMember(7)] public List<string> Modifiers { get; } = [];
    [ProtoMember(8)] public uint MaxScore { get; set; }
}

[ProtoContract]
public sealed class StreamReplayMetadata
{
    [ProtoMember(1)] public string ReplayVersion { get; set; } = "cocu-live-1";
    [ProtoMember(2)] public string LevelId { get; set; } = string.Empty;
    [ProtoMember(3)] public int Difficulty { get; set; }
    [ProtoMember(4)] public string Characteristic { get; set; } = "Standard";
    [ProtoMember(6)] public List<string> Modifiers { get; } = [];
    [ProtoMember(9)] public float InitialHeight { get; set; } = 1.7f;
    [ProtoMember(13)] public string GameVersion { get; set; } = string.Empty;
    [ProtoMember(14)] public string PluginVersion { get; set; } = string.Empty;
    [ProtoMember(15)] public string Platform { get; set; } = "PC";
    [ProtoMember(16)] public float SongSpeed { get; set; } = 1;
}

[ProtoContract]
public sealed class ReplayCursor
{
    [ProtoMember(1)] public ulong Sequence { get; set; }
    [ProtoMember(2)] public long SongTimeMs { get; set; }
    [ProtoMember(4)] public long ClientTimeUnixMs { get; set; }
}

[ProtoContract]
public sealed class ReplayChunk
{
    [ProtoMember(1)] public ReplayCursor? Cursor { get; set; }
    [ProtoMember(2)] public StreamReplayEventBatch? Events { get; set; }
}

[ProtoContract]
public sealed class StreamReplayEventBatch
{
    [ProtoMember(1)] public List<ReplayPoseFrame> PoseFrames { get; } = [];
    [ProtoMember(4)] public List<ReplayScoreEvent> ScoreEvents { get; } = [];
    [ProtoMember(5)] public List<ReplayComboEvent> ComboEvents { get; } = [];
    [ProtoMember(6)] public List<ReplayMultiplierEvent> MultiplierEvents { get; } = [];
    [ProtoMember(7)] public List<ReplayEnergyEvent> EnergyEvents { get; } = [];
    [ProtoMember(8)] public float MinTimeSeconds { get; set; }
    [ProtoMember(9)] public float MaxTimeSeconds { get; set; }
}

[ProtoContract]
public sealed class ReplayPoseFrame
{
    [ProtoMember(1)] public ReplayPose? Head { get; set; }
    [ProtoMember(2)] public ReplayPose? Left { get; set; }
    [ProtoMember(3)] public ReplayPose? Right { get; set; }
    [ProtoMember(4)] public int Fps { get; set; }
    [ProtoMember(5)] public float TimeSeconds { get; set; }
}

[ProtoContract]
public sealed class ReplayPose
{
    [ProtoMember(1)] public ReplayVector3? Position { get; set; }
    [ProtoMember(2)] public ReplayQuaternion? Rotation { get; set; }
}

[ProtoContract]
public sealed class ReplayVector3 { [ProtoMember(1)] public float X { get; set; } [ProtoMember(2)] public float Y { get; set; } [ProtoMember(3)] public float Z { get; set; } }
[ProtoContract]
public sealed class ReplayQuaternion { [ProtoMember(1)] public float X { get; set; } [ProtoMember(2)] public float Y { get; set; } [ProtoMember(3)] public float Z { get; set; } [ProtoMember(4)] public float W { get; set; } }
[ProtoContract]
public sealed class ReplayScoreEvent { [ProtoMember(1)] public int Score { get; set; } [ProtoMember(2)] public float TimeSeconds { get; set; } [ProtoMember(3)] public int ImmediateMaxPossibleScore { get; set; } }
[ProtoContract]
public sealed class ReplayComboEvent { [ProtoMember(1)] public int Combo { get; set; } [ProtoMember(2)] public float TimeSeconds { get; set; } }
[ProtoContract]
public sealed class ReplayMultiplierEvent { [ProtoMember(1)] public int Multiplier { get; set; } [ProtoMember(3)] public float TimeSeconds { get; set; } }
[ProtoContract]
public sealed class ReplayEnergyEvent { [ProtoMember(1)] public float Energy { get; set; } [ProtoMember(2)] public float TimeSeconds { get; set; } }

[ProtoContract]
public sealed class ReplayStreamEnd
{
    [ProtoMember(1)] public ReplayCursor? Cursor { get; set; }
    [ProtoMember(2)] public ReplayCompletion Completion { get; set; }
    [ProtoMember(3)] public ReplayScoreSummary? Score { get; set; }
    [ProtoMember(5)] public ulong ChunkCount { get; set; }
}

[ProtoContract]
public sealed class ReplayScoreSummary
{
    [ProtoMember(1)] public uint Score { get; set; }
    [ProtoMember(2)] public uint ModifiedScore { get; set; }
    [ProtoMember(3)] public uint MaxScore { get; set; }
    [ProtoMember(4)] public double Accuracy { get; set; }
    [ProtoMember(5)] public uint Combo { get; set; }
    [ProtoMember(7)] public bool FullCombo { get; set; }
}

public enum ReplayCompletion { Unspecified = 0, Passed = 1, Failed = 2, Quit = 3, Aborted = 4 }
