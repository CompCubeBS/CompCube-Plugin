using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube.Interfaces;
using CompCube.Server.Debug;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.Networking.Debug;

public class DebugServerListener : IServerListener
{
    [Inject] private readonly SiraLog _siraLog = null!;

    private bool _isConnected;

    public event Action<MatchCreatedPacket>? OnMatchCreated;
    public event Action<PlayerSelectedMapPacket>? OnPlayerSelectedMap;
    public event Action<RoundResultsPacket>? OnRoundResults;
    public event Action<StartPickPhasePacket>? OnPickPhaseStarted;
    public event Action<MatchFinishedPacket>? OnMatchFinished;
    public event Action<UpdateCardsPacket>? OnCardsUpdated;

    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnAbruptDisconnect;
    public bool Connected => _isConnected;

    public async Task ConnectAsync(string queue, Action? onConnectedCallback)
    {
        await Task.Delay(1000);

        _isConnected = true;
        
        onConnectedCallback?.Invoke();
        OnConnected?.Invoke();
        _siraLog.Info("connected");

        await Task.Delay(1000);
        OnMatchCreated?.Invoke(new MatchCreatedPacket(DebugApi.Self, DebugApi.DebugOpponent, DebugApi.Maps));
    }

    public Task DiscardMapsAsync(IReadOnlyCollection<VotingMap> maps)
    {
        if (!_isConnected) return Task.CompletedTask;
        OnPickPhaseStarted?.Invoke(new StartPickPhasePacket(DebugApi.Maps, true, 10f));
        return Task.CompletedTask;
    }

    public Task SelectMapAsync(VotingMap map) => Task.CompletedTask;
    
    public async Task SubmitScoreAsync(Score score)
    {
        if (!_isConnected) 
            return;
        
        OnRoundResults?.Invoke(new RoundResultsPacket(Score.Empty, Score.Empty, .5f, .5f));
        await Task.Delay(500);
        OnMatchFinished?.Invoke(new MatchFinishedPacket(100, false));
    }

    public Task HandleAbruptDisconnectionAsync(string reason)
    {
        if (!_isConnected) 
            return Task.CompletedTask;
        _isConnected = false;
        
        OnAbruptDisconnect?.Invoke(reason);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        if (!_isConnected) 
            return Task.CompletedTask;

        _isConnected = false;
        OnDisconnected?.Invoke();
        return Task.CompletedTask;
    }
}
