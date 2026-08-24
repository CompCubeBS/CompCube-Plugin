using System.Collections;
using System.Reflection;
using CompCube.Models;
using CompCube.Networking.Replay;
using UnityEngine;

namespace CompCube.Networking;

/** Samples compact poses and HUD state into the same protobuf chunks ChroViewer uses for TA live replays. */
public sealed class ReplayStreamer : MonoBehaviour
{
    private const int MaxFramesPerChunk = 24;
    private const float MaxChunkAgeSeconds = 0.25f;

    private ReplayPublisher _publisher = null!;
    private VotingMap _map = null!;
    private string _platformId = string.Empty;
    private string _pluginVersion = string.Empty;
    private string _streamId = string.Empty;
    private PlayerTransforms? _transforms;
    private AudioTimeSyncController? _audio;
    private ScoreController? _score;
    private ComboController? _combo;
    private GameEnergyCounter? _energy;
    private StreamReplayEventBatch _batch = new();
    private ulong _sequence = 1;
    private ulong _chunkCount;
    private float _batchStartedAt;
    private float _lastTime;
    private int _lastScore = int.MinValue;
    private int _lastCombo = int.MinValue;
    private int _lastMultiplier = int.MinValue;
    private float _lastEnergy = float.NaN;
    private bool _started;
    private bool _ended;

    public void Configure(ReplayPublisher publisher, VotingMap map, string platformId, string pluginVersion)
    {
        _publisher = publisher;
        _map = map;
        _platformId = platformId;
        _pluginVersion = pluginVersion;
        _streamId = $"cocu-pc-{Guid.NewGuid():N}";
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => _publisher != null && Resources.FindObjectsOfTypeAll<PlayerTransforms>().Any());
        yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().Any());
        yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<ScoreController>().Any());
        _transforms = Resources.FindObjectsOfTypeAll<PlayerTransforms>().First();
        _audio = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().First();
        _score = Resources.FindObjectsOfTypeAll<ScoreController>().First();
        _combo = Resources.FindObjectsOfTypeAll<ComboController>().FirstOrDefault();
        _energy = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().FirstOrDefault();
        _started = true;
        _ = _publisher.SendAsync(CreateStartPacket());
    }

    private void Update()
    {
        if (!_started || _ended || _audio == null || _transforms == null || _audio.songTime < 0) return;
        var time = _audio.songTime;
        _lastTime = Math.Max(_lastTime, time);
        if (_batch.PoseFrames.Count == 0)
        {
            _batch.MinTimeSeconds = time;
            _batchStartedAt = Time.realtimeSinceStartup;
        }
        _batch.MaxTimeSeconds = time;
        _batch.PoseFrames.Add(new ReplayPoseFrame
        {
            Head = Pose(_transforms.headPseudoLocalPos, _transforms.headPseudoLocalRot),
            Left = Pose(_transforms.leftHandPseudoLocalPos, _transforms.leftHandPseudoLocalRot),
            Right = Pose(_transforms.rightHandPseudoLocalPos, _transforms.rightHandPseudoLocalRot),
            Fps = Time.unscaledDeltaTime > 0 ? Mathf.RoundToInt(1f / Time.unscaledDeltaTime) : 90,
            TimeSeconds = time,
        });
        RecordHudChanges(time);
        if (_batch.PoseFrames.Count >= MaxFramesPerChunk || Time.realtimeSinceStartup - _batchStartedAt >= MaxChunkAgeSeconds)
            Flush();
    }

    private ReplayStreamPacket CreateStartPacket()
    {
        var difficulty = (int)_map.Difficulty * 2 + 1;
        var levelId = $"custom_level_{_map.Hash.ToUpperInvariant()}";
        var beatmap = new BeatmapIdentity
        {
            MapHash = _map.Hash.ToUpperInvariant(),
            LevelId = levelId,
            Difficulty = difficulty,
            DifficultyName = _map.Difficulty.ToString(),
            Characteristic = _map.Characteristic,
            MaxScore = (uint)Math.Max(0, _map.MaxScore),
        };
        beatmap.Modifiers.AddRange(_map.Modifiers);
        var metadata = new StreamReplayMetadata
        {
            LevelId = levelId,
            Difficulty = difficulty,
            Characteristic = _map.Characteristic,
            GameVersion = Application.version,
            PluginVersion = _pluginVersion,
            SongSpeed = _map.Modifiers.Contains("SS") ? 0.8f : _map.Modifiers.Contains("SFS") ? 1.5f : _map.Modifiers.Contains("FS") ? 1.2f : 1f,
        };
        metadata.Modifiers.AddRange(_map.Modifiers);
        return new ReplayStreamPacket
        {
            StreamId = _streamId,
            PlayerId = _platformId,
            Start = new ReplayStreamStart
            {
                ClientStartTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                GameSessionId = Guid.NewGuid().ToString("N"),
                Player = new PlayerIdentity
                {
                    PlayerId = _platformId,
                    GameVersion = Application.version,
                    ClientVersion = _pluginVersion,
                },
                Beatmap = beatmap,
                ReplayMetadata = metadata,
            },
        };
    }

    private void RecordHudChanges(float time)
    {
        var score = _score?.modifiedScore ?? 0;
        if (score != _lastScore)
        {
            _lastScore = score;
            _batch.ScoreEvents.Add(new ReplayScoreEvent
            {
                Score = score,
                ImmediateMaxPossibleScore = _score?.immediateMaxPossibleModifiedScore ?? 0,
                TimeSeconds = time,
            });
        }
        var combo = GetInt(_combo, "_combo", "combo");
        if (combo != _lastCombo)
        {
            _lastCombo = combo;
            _batch.ComboEvents.Add(new ReplayComboEvent { Combo = combo, TimeSeconds = time });
        }
        var multiplier = GetInt(GetMember(_score, "_scoreMultiplierCounter"), "multiplier");
        if (multiplier != _lastMultiplier)
        {
            _lastMultiplier = multiplier;
            _batch.MultiplierEvents.Add(new ReplayMultiplierEvent { Multiplier = multiplier, TimeSeconds = time });
        }
        var energy = _energy?.energy ?? 0;
        if (float.IsNaN(_lastEnergy) || Math.Abs(energy - _lastEnergy) > 0.0001f)
        {
            _lastEnergy = energy;
            _batch.EnergyEvents.Add(new ReplayEnergyEvent { Energy = energy, TimeSeconds = time });
        }
    }

    private void Flush()
    {
        if (_batch.PoseFrames.Count == 0) return;
        _ = _publisher.SendAsync(new ReplayStreamPacket
        {
            StreamId = _streamId,
            PlayerId = _platformId,
            Chunk = new ReplayChunk
            {
                Cursor = Cursor(_sequence++, _batch.MaxTimeSeconds),
                Events = _batch,
            },
        });
        _chunkCount++;
        _batch = new StreamReplayEventBatch();
    }

    public void Complete(LevelCompletionResults results)
    {
        if (!_started || _ended) return;
        _ended = true;
        Flush();
        var completion = results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared
            ? ReplayCompletion.Passed
            : results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed
                ? ReplayCompletion.Failed
                : ReplayCompletion.Quit;
        _ = _publisher.SendAsync(new ReplayStreamPacket
        {
            StreamId = _streamId,
            PlayerId = _platformId,
            End = new ReplayStreamEnd
            {
                Cursor = Cursor(_sequence++, _lastTime),
                Completion = completion,
                ChunkCount = _chunkCount,
                Score = new ReplayScoreSummary
                {
                    Score = (uint)Math.Max(0, results.multipliedScore),
                    ModifiedScore = (uint)Math.Max(0, results.modifiedScore),
                    MaxScore = (uint)Math.Max(0, _map.MaxScore),
                    Accuracy = _map.MaxScore <= 0 ? 0 : (double)results.multipliedScore / _map.MaxScore,
                    Combo = (uint)Math.Max(0, _lastCombo),
                    FullCombo = results.fullCombo,
                },
            },
        });
    }

    private static ReplayCursor Cursor(ulong sequence, float time) => new()
    {
        Sequence = sequence,
        SongTimeMs = (long)Math.Round(time * 1000),
        ClientTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    };

    private static ReplayPose Pose(Vector3 position, Quaternion rotation) => new()
    {
        Position = new ReplayVector3 { X = position.x, Y = position.y, Z = position.z },
        Rotation = new ReplayQuaternion { X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w },
    };

    private static object? GetMember(object? target, params string[] names)
    {
        if (target == null) return null;
        for (var type = target.GetType(); type != null; type = type.BaseType)
        foreach (var name in names)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(target);
            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) return property.GetValue(target, null);
        }
        return null;
    }

    private static int GetInt(object? target, params string[] names)
    {
        try { return Convert.ToInt32(GetMember(target, names) ?? 0); }
        catch { return 0; }
    }
}
