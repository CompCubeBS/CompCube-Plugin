using System.Net.Http;
using System.Text;
using CompCube.Configuration;
using Newtonsoft.Json;

namespace CompCube.Networking;

/** Requests the short-lived BeatKhana game token used by both Socket.IO and the replay stream. */
public sealed class BeatKhanaGameAuth
{
    private static readonly HttpClient Http = new();
    private readonly PluginConfig _config;
    private readonly IPlatformUserModel _platformUserModel;

    public BeatKhanaGameAuth(PluginConfig config, IPlatformUserModel platformUserModel)
    {
        _config = config;
        _platformUserModel = platformUserModel;
    }

    public sealed class AuthResponse
    {
        [JsonProperty("token")] public string Token { get; set; } = string.Empty;
        [JsonProperty("platform")] public string Platform { get; set; } = string.Empty;
        [JsonProperty("platformId")] public string PlatformId { get; set; } = string.Empty;
        [JsonProperty("userGuid")] public string? UserGuid { get; set; }
        [JsonProperty("discordId")] public string? DiscordId { get; set; }
    }

    /** Exchanges the game platform's signed ticket for a token carrying the CompCube capability. */
    public async Task<AuthResponse> RequestTokenAsync(CancellationToken cancellationToken = default)
    {
        var userInfo = await _platformUserModel.GetUserInfo(cancellationToken);
        var isSteam = userInfo.platform == UserInfo.Platform.Steam;
        var ticket = await GetPlatformTicketAsync(userInfo, isSteam, cancellationToken);
        var request = new
        {
            provider = isSteam ? "steamTicket" : "oculusTicket",
            ticket,
            scopes = new[] { "compcube" },
        };

        using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        using var response = await Http.PostAsync(
            $"{_config.BeatKhanaApiIP.TrimEnd('/')}/game/requestAuthToken",
            content,
            cancellationToken);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"BeatKhana auth failed ({(int)response.StatusCode}): {json}");

        return JsonConvert.DeserializeObject<AuthResponse>(json)
            ?? throw new InvalidOperationException("BeatKhana returned an empty auth response.");
    }

    private async Task<string> GetPlatformTicketAsync(UserInfo userInfo, bool isSteam, CancellationToken cancellationToken)
    {
        var provider = new PlatformAuthenticationTokenProvider(_platformUserModel, userInfo);
        var ticket = isSteam
            ? (await provider.GetAuthenticationToken()).sessionToken
            : (await provider.GetXPlatformAccessToken(cancellationToken)).token;

        if (string.IsNullOrWhiteSpace(ticket))
            throw new InvalidOperationException("The game platform did not return an authentication ticket.");
        return ticket;
    }
}
