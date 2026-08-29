using System.Collections;
using System.Net;
using System.Net.Http;
using CompCube.Models;
using CompCube.Configuration;
using CompCube.Interfaces;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;
using Queue = CompCube.Models.Queue;
using ServerStatus = CompCube.Models.ServerStatus;

namespace CompCube.Server
{
    public class Api : IApi
    {
        private readonly HttpClient _client;

        public Api(PluginConfig config)
        {
            var handler = new HttpClientHandler();
            
            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri($"{config.ApiIP}/", UriKind.Absolute);
        }

        /** Loads a public user from the TypeScript backend and converts it into the UI model. */
        public async Task<CompCube.Models.UserInfo?> GetUserInfo(string id)
        {
            var response = await _client.GetAsync($"/user/id/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var user = JsonConvert.DeserializeObject<UserResponse>(await response.Content.ReadAsStringAsync());
            return user == null ? null : ToUserInfo(user);
        }

        /** Loads one page of the current season leaderboard. */
        public async Task<CompCube.Models.UserInfo[]?> GetLeaderboardRange(int start, int range)
        {
            var response = await _client.GetAsync($"/leaderboard/range?start={start}&range={range}");
            response.EnsureSuccessStatusCode();
            var users = JsonConvert.DeserializeObject<LeaderboardUserResponse[]>(await response.Content.ReadAsStringAsync());
            return users?.Select(ToUserInfo).ToArray();
        }

        /** Loads the leaderboard entries surrounding one platform user. */
        public async Task<CompCube.Models.UserInfo[]?> GetAroundUser(string id)
        {
            var response = await _client.GetAsync($"/leaderboard/aroundUser/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var users = JsonConvert.DeserializeObject<LeaderboardUserResponse[]>(await response.Content.ReadAsStringAsync());
            return users?.Select(ToUserInfo).ToArray();
        }

        public async Task<ServerStatus?> GetServerStatus()
        {
            var response = await _client.GetAsync("/server/status");
            if (!response.IsSuccessStatusCode) return null;
            return JsonConvert.DeserializeObject<ServerStatus>(await response.Content.ReadAsStringAsync());
        }

        public async Task<Queue[]?> GetQueues()
        {
            var response  = await _client.GetAsync("/queues");
            
            return response.StatusCode == HttpStatusCode.NotFound ? null : JsonConvert.DeserializeObject<Queue[]>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string[]?> GetMapHashes()
        {
            var response = await _client.GetAsync("/maps/hashes");
            return JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());
        }

        public async Task<EventData[]?> GetEvents()
        {
            // Events belonged to the retired C# backend and are not part of the new game client API.
            await Task.CompletedTask;
            return [];
        }

        public async Task<byte[]?> DownloadBeatmap(string hash)
        {
            var response  = await _client.GetAsync($"/maps/download/{hash}");
            
            if (!response.IsSuccessStatusCode)
                return null;
            
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]?> DownloadUserProfilePicture(CompCube.Models.UserInfo userInfo)
        {
            var response = await _client.GetAsync(userInfo.ProfilePictureLink);

            if (!response.IsSuccessStatusCode)
                return null;
            
            return await response.Content.ReadAsByteArrayAsync();
        }

        private static CompCube.Models.UserInfo ToUserInfo(UserResponse user) => new(
            user.Username,
            user.PlatformId ?? user.Guid,
            0,
            null,
            0,
            user.DiscordId,
            user.Banned,
            0,
            0,
            0,
            0,
            user.AvatarUrl);

        private static CompCube.Models.UserInfo ToUserInfo(LeaderboardUserResponse user) => new(
            user.Username,
            user.PlatformId ?? user.UserGuid,
            user.Mmr,
            null,
            user.Rank,
            null,
            false,
            user.Wins,
            user.TotalGames,
            user.WinStreak,
            user.BestWinStreak,
            user.AvatarUrl);

        private sealed class UserResponse
        {
            public string Guid { get; set; } = string.Empty;
            public string? PlatformId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
            public string? DiscordId { get; set; }
            public bool Banned { get; set; }
        }

        private sealed class LeaderboardUserResponse
        {
            public string UserGuid { get; set; } = string.Empty;
            public string? PlatformId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
            public int Mmr { get; set; }
            public int Rank { get; set; }
            public int Wins { get; set; }
            public int TotalGames { get; set; }
            public int WinStreak { get; set; }
            public int BestWinStreak { get; set; }
        }
    }
}