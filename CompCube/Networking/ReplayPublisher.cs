using System.Net.WebSockets;
using CompCube.Models;
using CompCube.Configuration;
using CompCube.Networking.Replay;
using IPA.Loader;
using ProtoBuf;
using UnityEngine;

namespace CompCube.Networking;

/** Publishes bounded protobuf frames to the authenticated raw replay WebSocket. */
public sealed class ReplayPublisher : IDisposable
{
    private readonly PluginConfig _config;
    private readonly BeatKhanaGameAuth _auth;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private ClientWebSocket? _socket;
    private ReplayStreamer? _streamer;

    public ReplayPublisher(PluginConfig config, BeatKhanaGameAuth auth)
    {
        _config = config;
        _auth = auth;
    }

    /** Opens the publishing socket and starts collecting this map's broadcast-friendly replay frames. */
    public async Task StartAsync(VotingMap map)
    {
        await StopAsync();
        var auth = await _auth.RequestTokenAsync();
        var version = PluginManager.GetPluginFromId("CompCube").HVersion.ToString();
        _socket = new ClientWebSocket();
        _socket.Options.SetRequestHeader("Authorization", $"Bearer {auth.Token}");
        _socket.Options.SetRequestHeader("X-CompCube-Plugin-Version", version);
        await _socket.ConnectAsync(
            new Uri($"{_config.WebsocketIp.TrimEnd('/')}/live/u/{auth.PlatformId}"),
            CancellationToken.None);

        var gameObject = new GameObject("CompCube Replay Streamer");
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        _streamer = gameObject.AddComponent<ReplayStreamer>();
        _streamer.Configure(this, map, auth.PlatformId, version);
    }

    /** Serializes one replay packet and sends it without allowing concurrent WebSocket writes. */
    public async Task SendAsync(ReplayStreamPacket packet)
    {
        if (_socket?.State != WebSocketState.Open) return;
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, packet);
        var bytes = stream.ToArray();
        await _sendLock.WaitAsync();
        try
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Complete(LevelCompletionResults results) => _streamer?.Complete(results);

    private async Task StopAsync()
    {
        if (_streamer != null) UnityEngine.Object.Destroy(_streamer.gameObject);
        _streamer = null;
        if (_socket?.State == WebSocketState.Open)
            await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Map ended", CancellationToken.None);
        _socket?.Dispose();
        _socket = null;
    }

    public void Dispose()
    {
        _streamer = null;
        _socket?.Dispose();
        _sendLock.Dispose();
    }
}
