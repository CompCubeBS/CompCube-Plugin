using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube.Interfaces;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.Server.Debug;

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
        await SendPacketAsync(new JoinRequestPacket(DebugApi.Self.Username, DebugApi.Self.UserId, queue));
    }

    public async Task SendPacketAsync(UserPacket packet)
    {
        _siraLog.Info($"sent packet {packet.PacketType.ToString()}");
        
        if (!_isConnected)
        {
            _siraLog.Info("tried to send packet when not connected!");
            return;
        }

        switch (packet.PacketType)
        {
            case UserPacket.UserPacketTypes.JoinRequest:
                OnMatchCreated?.Invoke(new MatchCreatedPacket(DebugApi.Self, DebugApi.DebugOpponent, DebugApi.Maps));

                // await Task.Delay(2000);
                break;
            case UserPacket.UserPacketTypes.DiscardMaps:
                OnPickPhaseStarted?.Invoke(new StartPickPhasePacket(DebugApi.Maps, true, 10f));
                break;
            case UserPacket.UserPacketTypes.MapSelection:
                
                break;
            case UserPacket.UserPacketTypes.ScoreSubmission:
                OnRoundResults?.Invoke(new RoundResultsPacket(Score.Empty, Score.Empty, .5f, .5f));

                await Task.Delay(500);
                
                OnMatchFinished?.Invoke(new MatchFinishedPacket(100, false));
                break;
        }
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