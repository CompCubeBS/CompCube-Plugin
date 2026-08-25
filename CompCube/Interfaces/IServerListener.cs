using CompCube.Models;

namespace CompCube.Interfaces;

public interface IServerListener
{
    public event Action<MatchCreatedMessage> OnMatchCreated;
    
    public event Action<PlayerSelectedMapMessage> OnPlayerSelectedMap;
    
    public event Action<RoundResultsMessage> OnRoundResults;
    
    public event Action<PickPhaseMessage> OnPickPhaseStarted;

    public event Action<MatchFinishedMessage> OnMatchFinished;
    
    public event Action<CardsUpdatedMessage> OnCardsUpdated;

    public event Action OnConnected;
    
    public event Action OnDisconnected;

    public event Action<string> OnAbruptDisconnect;
    
    public bool Connected { get; }

    public Task ConnectAsync(string queue, Action? onConnectedCallback = null);

    public Task DiscardMapsAsync(IReadOnlyCollection<VotingMap> maps);

    public Task SelectMapAsync(VotingMap map);

    public Task SubmitScoreAsync(ScoreSubmission score);

    public Task DisconnectAsync();
    
    public Task HandleAbruptDisconnectionAsync(string reason);
}
