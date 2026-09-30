using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;

namespace CompCube.Interfaces;

public interface IServerListener
{
    public event Action<MatchCreatedPacket> OnMatchCreated;
    
    public event Action<PlayerSelectedMapPacket> OnPlayerSelectedMap;
    
    public event Action<RoundResultsPacket> OnRoundResults;
    
    public event Action<StartPickPhasePacket> OnPickPhaseStarted;

    public event Action<MatchFinishedPacket> OnMatchFinished;
    
    public event Action<UpdateCardsPacket> OnCardsUpdated;

    public event Action OnConnected;
    
    public event Action OnDisconnected;

    public event Action<string> OnAbruptDisconnect;
    
    public bool Connected { get; }

    public Task ConnectAsync(string queue, Action? onConnectedCallback = null);

    public Task DiscardMapsAsync(IReadOnlyCollection<VotingMap> maps);

    public Task SelectMapAsync(VotingMap map);

    public Task SubmitScoreAsync(Score score);

    public Task DisconnectAsync();
    
    public Task HandleAbruptDisconnectionAsync(string reason);
}
