using CompCube.Models;
using CompCube.Interfaces;
using IPA.Utilities;
using ServerState = CompCube.Models.ServerState;
using ServerStatus = CompCube.Models.ServerStatus;

namespace CompCube.Server.Debug;

public class DebugApi : IApi
{
    public static readonly VotingMap[] Maps =
    [
        new("44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Tech), 
        new("44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.MidSpeed),
        new("44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Extreme),
        new("44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Speed),
        new("44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Accuracy)
    ];

    public static readonly CompCube.Models.UserInfo DebugOpponent = new("debugOpponent", "1", 1000, null, 2, null,
        false, 0, 0, 0, 0);

    public static readonly CompCube.Models.UserInfo Self = new(
        "self",
        "0",
        1000,
        null,
        1,
        null,
        false, 0, 0, 0, 0);
    
    public async Task<CompCube.Models.UserInfo?> GetUserInfo(string id)
    {
        await Task.Delay(1000);
        return Self;
    }

    public Task<CompCube.Models.UserInfo[]?> GetLeaderboardRange(int start, int range)
    {
        var info = new List<CompCube.Models.UserInfo>()
        {
            DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self
        };
        return Task.FromResult(info.ToArray());
    }

    public Task<CompCube.Models.UserInfo[]?> GetAroundUser(string id)
    {
        return Task.FromResult(Array.Empty<CompCube.Models.UserInfo>());
    }

    public Task<ServerStatus?> GetServerStatus()
    {
        return Task.FromResult(new ServerStatus([UnityGame.GameVersion.ToString()], [IPA.Loader.PluginManager.GetPluginFromId("CompCube").HVersion.ToString()],
            ServerState.Online));
    }

    public async Task<Queue[]?> GetQueues()
    {
        await Task.Delay(500);

        return [new Queue("test", "test", "test", "test", false, true)];
    }

    public async Task<string[]?> GetMapHashes()
    {
        await Task.Delay(1000);
        return Maps.Select(i => i.Hash).ToArray();
    }

    public async Task<EventData[]?> GetEvents()
    {
        await Task.Delay(500);
        return [];
    }

    public async Task<byte[]?> DownloadBeatmap(string hash)
    {
        await Task.Delay(500);
        return [];
    }

    public Task<byte[]?> DownloadUserProfilePicture(CompCube.Models.UserInfo userInfo)
    {
        return Task.FromResult<byte[]?>([]);
    }
}
