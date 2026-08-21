using System.Net.WebSockets;
using System.Text;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube.Configuration;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.Networking
{
    public class ServerListener : IServerListener, IDisposable
    {
        [Inject] private readonly PluginConfig _config = null!;
        [Inject] private readonly SiraLog _siraLog = null!;

        private ClientWebSocket _client = new();
        
        public event Action<MatchCreatedPacket>? OnMatchCreated;
        public event Action<PlayerSelectedMapPacket>? OnPlayerSelectedMap;
        public event Action<RoundResultsPacket>? OnRoundResults;
        public event Action<StartPickPhasePacket>? OnPickPhaseStarted;
        public event Action<MatchFinishedPacket>? OnMatchFinished;
        
        public event Action<UpdateCardsPacket>? OnCardsUpdated; 
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<string>? OnAbruptDisconnect;

        private bool _shouldListenToServer;


        [Inject] private readonly UserModelWrapper _userModelWrapper = null!;

        public bool Connected => _client.State == WebSocketState.Open;
        
        private CancellationTokenSource _cancellationTokenSource = new();
        
        public async Task ConnectAsync(string queueEndpoint, Action? onConnectedCallback)
        {
            if (Connected)
            {
                _siraLog.Error("Tried to connect to server while already connected!");
                return;
            }

            try
            {
                _client = new ClientWebSocket();
                
                _client.Options.SetRequestHeader("UserId", _userModelWrapper.UserId);
                _client.Options.SetRequestHeader("UserName", _userModelWrapper.UserName);
                
                _cancellationTokenSource = new CancellationTokenSource();
                await _client.ConnectAsync(new Uri($"{_config.WebsocketIp}/{queueEndpoint}", UriKind.Absolute), _cancellationTokenSource.Token);
                
                onConnectedCallback?.Invoke();
                
                _shouldListenToServer = true;
                
                while (_shouldListenToServer)
                    await ListenToServerAsync();
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        private async Task ListenToServerAsync()
        {
            try
            {
                var data = new byte[4096];

                var result = await _client.ReceiveAsync(new ArraySegment<byte>(data), _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleAbruptDisconnectionAsync("Disconnected");
                    return;
                }

                var json = Encoding.UTF8.GetString(data);

                if (json == "")
                    return;

                var packet = ServerPacket.Deserialize(json);

                switch (packet.PacketType)
                {
                    case ServerPacket.ServerPacketTypes.MatchCreated:
                        OnMatchCreated?.Invoke(packet as MatchCreatedPacket);
                        break;
                    case ServerPacket.ServerPacketTypes.PlayerSelectedMap:
                        OnPlayerSelectedMap?.Invoke(packet as PlayerSelectedMapPacket);
                        break;
                    case ServerPacket.ServerPacketTypes.RoundResults:
                        OnRoundResults?.Invoke(packet as RoundResultsPacket);
                        break;
                    case ServerPacket.ServerPacketTypes.StartPickPhase:
                        OnPickPhaseStarted?.Invoke(packet as StartPickPhasePacket);
                        break;
                    case ServerPacket.ServerPacketTypes.MatchFinished:
                        OnMatchFinished?.Invoke(packet as MatchFinishedPacket);
                        
                        await StopListeningToServerAsync();
                        break;
                    case ServerPacket.ServerPacketTypes.UpdateCards:
                        OnCardsUpdated?.Invoke(packet as UpdateCardsPacket);
                        break;
                    case ServerPacket.ServerPacketTypes.AbruptDisconnection:
                        var disconnectPacket = packet as AbruptDisconnectionPacket;
                        _siraLog.Notice("Disconnected from server: " + disconnectPacket?.Reason);
                        await HandleAbruptDisconnectionAsync(disconnectPacket!.Reason);
                        break;
                    default:
                        throw new Exception("Could not get packet type!");
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                _siraLog.Error(e);
                await HandleAbruptDisconnectionAsync("Unhandled exception, please check your logs!");
            }
        }

        public async Task SendPacketAsync(UserPacket packet)
        {
            try
            {
                var buffer = packet.SerializeToBytes();
                await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        private async Task StopListeningToServerAsync()
        {
            _shouldListenToServer = false;
            _cancellationTokenSource.Cancel();
            await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", CancellationToken.None);
            _client.Dispose();
        }

        public async Task DisconnectAsync()
        {
            await SendPacketAsync(new ClientDisconnectPacket());
            await StopListeningToServerAsync();
            OnDisconnected?.Invoke();
        }

        public async Task HandleAbruptDisconnectionAsync(string reason)
        {
            OnAbruptDisconnect?.Invoke(reason);
            await StopListeningToServerAsync();
        }

        public void Dispose() => _client.Dispose();
    }
}