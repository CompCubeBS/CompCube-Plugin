using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;

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

    public Task SendPacketAsync(UserPacket packet);

    public Task DisconnectAsync();
    
    public Task HandleAbruptDisconnectionAsync(string reason);
}