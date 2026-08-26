using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.Models;
using IPA.Loader;
using SiraUtil.Logging;
using SocketIOClient;
using SocketIOClient.Transport;
using Zenject;

namespace CompCube.Networking;

/** Owns the authenticated Socket.IO session and translates backend events into the existing game flow. */
public sealed class ServerListener : IServerListener, IDisposable
{
    [Inject] private readonly PluginConfig _config = null!;
    [Inject] private readonly BeatKhanaGameAuth _auth = null!;
    [Inject] private readonly SiraLog _siraLog = null!;

    private SocketIO? _socket;
    private string? _matchGuid;
    private string? _roundGuid;
    private string? _redUserGuid;
    private string? _blueUserGuid;

    public event Action<MatchCreatedMessage>? OnMatchCreated;
    public event Action<PlayerSelectedMapMessage>? OnPlayerSelectedMap;
    public event Action<RoundResultsMessage>? OnRoundResults;
    public event Action<PickPhaseMessage>? OnPickPhaseStarted;
    public event Action<MatchFinishedMessage>? OnMatchFinished;
    public event Action<CardsUpdatedMessage>? OnCardsUpdated;
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnAbruptDisconnect;

    public bool Connected => _socket?.Connected == true;

    /** Authenticates through BeatKhana, opens Socket.IO and joins the selected queue. */
    public async Task ConnectAsync(string queueEndpoint, Action? onConnectedCallback)
    {
        if (Connected)
        {
            _siraLog.Error("Tried to connect to the server while already connected!");
            return;
        }

        try
        {
            var auth = await _auth.RequestTokenAsync();
            _socket = new SocketIO(new Uri(_config.WebsocketIp), new SocketIOOptions
            {
                Transport = TransportProtocol.WebSocket,
                Reconnection = false,
                Auth = new Dictionary<string, string>
                {
                    ["accessToken"] = auth.Token,
                    ["clientType"] = "plugin",
                    ["pluginVersion"] = PluginManager.GetPluginFromId("CompCube").HVersion.ToString(),
                },
            });
            RegisterServerEvents(_socket);
            await _socket.ConnectAsync();
            var queue = queueEndpoint.StartsWith("queue/", StringComparison.OrdinalIgnoreCase)
                ? queueEndpoint.Substring("queue/".Length)
                : queueEndpoint;
            await EmitAcknowledgedAsync<object>("joinQueue", new { queue });
            OnConnected?.Invoke();
            onConnectedCallback?.Invoke();
        }
        catch (Exception exception)
        {
            _siraLog.Error(exception);
            await HandleAbruptDisconnectionAsync(exception.Message);
            throw;
        }
    }

    /** Submits the maps discarded by the local player. */
    public async Task DiscardMapsAsync(IReadOnlyCollection<VotingMap> maps)
    {
        RequireMatch();
        await EmitAcknowledgedAsync<object>("discardMaps", new
        {
            matchGuid = _matchGuid,
            mapGuids = maps.Select(map => map.Guid).ToArray(),
        });
    }

    /** Selects one map for the local player's pick. */
    public async Task SelectMapAsync(VotingMap map)
    {
        RequireMatch();
        await EmitAcknowledgedAsync<object>("selectMap", new { matchGuid = _matchGuid, mapGuid = map.Guid });
    }

    /** Submits the completed round score using the current server-issued round identifier. */
    public async Task SubmitScoreAsync(ScoreSubmission score)
    {
        RequireMatch();
        if (string.IsNullOrWhiteSpace(_roundGuid))
            throw new InvalidOperationException("The server has not started a score round.");
        await EmitAcknowledgedAsync<object>("submitScore", new
        {
            matchGuid = _matchGuid,
            roundGuid = _roundGuid,
            rawScore = score.RawScore,
            modifiedScore = score.ModifiedScore,
            noFailTriggered = score.NoFailTriggered,
            proMode = score.ProMode,
            missCount = score.MissCount,
            fullCombo = score.FullCombo,
        });
    }

    /** Forfeits an active match, or leaves the queue, before closing the transport. */
    public async Task DisconnectAsync()
    {
        if (_socket == null) return;
        try
        {
            if (_socket.Connected) await ForfeitOrLeaveAsync("player_left");
            if (_socket.Connected) await _socket.DisconnectAsync();
        }
        finally
        {
            _socket.Dispose();
            _socket = null;
            ClearMatch();
            OnDisconnected?.Invoke();
        }
    }

    /** Moves the UI out of its connected state after a transport or authentication failure. */
    public async Task HandleAbruptDisconnectionAsync(string reason)
    {
        OnAbruptDisconnect?.Invoke(reason);
        if (_socket?.Connected == true) await _socket.DisconnectAsync();
        _socket?.Dispose();
        _socket = null;
        ClearMatch();
    }

    private void RegisterServerEvents(SocketIO socket)
    {
        socket.OnDisconnected += (_, reason) => OnAbruptDisconnect?.Invoke(reason);
        socket.On("matchCreated", response =>
        {
            var value = response.GetValue<MatchCreatedEvent>();
            _matchGuid = value.MatchGuid;
            _redUserGuid = value.Red.Guid;
            _blueUserGuid = value.Blue.Guid;
            OnMatchCreated?.Invoke(new MatchCreatedMessage(ToUser(value.Red), ToUser(value.Blue), value.InitialMaps.Select(ToMap).ToArray()));
        });
        socket.On("cardsUpdated", response =>
        {
            var value = response.GetValue<CardsUpdatedEvent>();
            OnCardsUpdated?.Invoke(new CardsUpdatedMessage(value.Maps.Select(ToMap).ToArray()));
        });
        socket.On("pickPhaseStarted", response =>
        {
            var value = response.GetValue<PickPhaseEvent>();
            OnPickPhaseStarted?.Invoke(new PickPhaseMessage(
                value.AvailableMaps.Select(ToMap).ToArray(), value.IsOwnPick, (float)value.DamageMultiplier));
        });
        socket.On("playerSelectedMap", response =>
        {
            var value = response.GetValue<SelectedMapEvent>();
            OnPlayerSelectedMap?.Invoke(new PlayerSelectedMapMessage(ToMap(value.Map)));
        });
        socket.On("startMap", response => _roundGuid = response.GetValue<StartMapEvent>().RoundGuid);
        socket.On("roundResults", response =>
        {
            var value = response.GetValue<RoundResultsEvent>();
            OnRoundResults?.Invoke(new RoundResultsMessage(
                ToScore(value.Scores.FirstOrDefault(score => score.UserGuid == _redUserGuid)),
                ToScore(value.Scores.FirstOrDefault(score => score.UserGuid == _blueUserGuid)),
                (float)value.RedHealth,
                (float)value.BlueHealth));
        });
        socket.On("matchFinished", response =>
        {
            var value = response.GetValue<MatchFinishedEvent>();
            OnMatchFinished?.Invoke(new MatchFinishedMessage(value.MmrChange, value.Result, value.Reason));
            ClearMatch();
        });
    }

    private async Task ForfeitOrLeaveAsync(string reason)
    {
        if (_socket?.Connected != true) return;
        if (!string.IsNullOrWhiteSpace(_matchGuid))
            await EmitAcknowledgedAsync<object>("forfeit", new { matchGuid = _matchGuid, reason });
        else
            await EmitAcknowledgedAsync<object>("leaveQueue", new { });
    }

    private async Task<T?> EmitAcknowledgedAsync<T>(string eventName, object payload)
    {
        RequireConnection();
        var completion = new TaskCompletionSource<T?>();
        await _socket!.EmitAsync(eventName, response =>
        {
            var ack = response.GetValue<Acknowledgement<T>>();
            if (ack.Ok) completion.TrySetResult(ack.Data);
            else completion.TrySetException(new InvalidOperationException(ack.Error?.Message ?? "The server rejected the action."));
        }, payload);
        return await completion.Task;
    }

    private void RequireConnection()
    {
        if (_socket?.Connected != true) throw new InvalidOperationException("The CompCube socket is not connected.");
    }

    private void RequireMatch()
    {
        if (string.IsNullOrWhiteSpace(_matchGuid)) throw new InvalidOperationException("There is no active CompCube match.");
    }

    private void ClearMatch()
    {
        _matchGuid = null;
        _roundGuid = null;
        _redUserGuid = null;
        _blueUserGuid = null;
    }

    private static CompCube.Models.UserInfo ToUser(PacketUser user) => new(user.Username, user.PlatformId, 0, null, 0, null, false, 0, 0, 0, 0);

    private static VotingMap ToMap(PacketMap map)
    {
        Enum.TryParse(map.Difficulty, true, out VotingMap.DifficultyType difficulty);
        return new VotingMap(map.Hash, difficulty, VotingMap.Category.Special, map.Guid, map.Characteristic,
            map.Modifiers, map.DurationSeconds, map.MaxScore);
    }

    private static Score ToScore(PacketScore? score) => score == null
        ? Score.Empty
        : new Score(score.ModifiedScore, (float)score.Accuracy, score.ProMode, score.MissCount, score.FullCombo);

    public void Dispose() => _socket?.Dispose();

    private sealed class Acknowledgement<T> { public bool Ok { get; set; } public T? Data { get; set; } public ErrorDetails? Error { get; set; } }
    private sealed class ErrorDetails { public string Message { get; set; } = string.Empty; }
    private sealed class PacketUser { public string Guid { get; set; } = string.Empty; public string PlatformId { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; }
    private sealed class PacketMap
    {
        public string Guid { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Characteristic { get; set; } = "Standard";
        public string Difficulty { get; set; } = "ExpertPlus";
        public string[] Modifiers { get; set; } = [];
        public int DurationSeconds { get; set; }
        public int MaxScore { get; set; }
    }
    private sealed class PacketScore
    {
        public string UserGuid { get; set; } = string.Empty;
        public int ModifiedScore { get; set; }
        public double Accuracy { get; set; }
        public bool ProMode { get; set; }
        public int MissCount { get; set; }
        public bool FullCombo { get; set; }
    }
    private sealed class MatchCreatedEvent { public string MatchGuid { get; set; } = string.Empty; public PacketUser Red { get; set; } = new(); public PacketUser Blue { get; set; } = new(); public PacketMap[] InitialMaps { get; set; } = []; }
    private sealed class CardsUpdatedEvent { public PacketMap[] Maps { get; set; } = []; }
    private sealed class PickPhaseEvent { public bool IsOwnPick { get; set; } public PacketMap[] AvailableMaps { get; set; } = []; public double DamageMultiplier { get; set; } }
    private sealed class SelectedMapEvent { public PacketMap Map { get; set; } = new(); }
    private sealed class StartMapEvent { public string RoundGuid { get; set; } = string.Empty; }
    private sealed class RoundResultsEvent { public double RedHealth { get; set; } public double BlueHealth { get; set; } public PacketScore[] Scores { get; set; } = []; }
    private sealed class MatchFinishedEvent { public string Result { get; set; } = "loss"; public int MmrChange { get; set; } public string? Reason { get; set; } }
}
