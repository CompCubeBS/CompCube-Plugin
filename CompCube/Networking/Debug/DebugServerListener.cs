using CompCube.Interfaces;
using CompCube.Models;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.Server.Debug;

public class DebugServerListener : IServerListener
{
    [Inject] private readonly SiraLog _siraLog = null!;

    private bool _isConnected;

    public event Action<MatchCreatedMessage>? OnMatchCreated;
    public event Action<PlayerSelectedMapMessage>? OnPlayerSelectedMap;
    public event Action<RoundResultsMessage>? OnRoundResults;
    public event Action<PickPhaseMessage>? OnPickPhaseStarted;
    public event Action<MatchFinishedMessage>? OnMatchFinished;
    public event Action<CardsUpdatedMessage>? OnCardsUpdated;

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
        OnMatchCreated?.Invoke(new MatchCreatedMessage(DebugApi.Self, DebugApi.DebugOpponent, DebugApi.Maps));
    }

    public Task DiscardMapsAsync(IReadOnlyCollection<VotingMap> maps)
    {
        if (!_isConnected) return Task.CompletedTask;
        OnPickPhaseStarted?.Invoke(new PickPhaseMessage(DebugApi.Maps, true, 10f));
        return Task.CompletedTask;
    }

    public Task SelectMapAsync(VotingMap map) => Task.CompletedTask;

    public async Task SubmitScoreAsync(ScoreSubmission score)
    {
        if (!_isConnected) return;
        OnRoundResults?.Invoke(new RoundResultsMessage(Score.Empty, Score.Empty, .5f, .5f));
        await Task.Delay(500);
        OnMatchFinished?.Invoke(new MatchFinishedMessage(100, "loss"));
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
