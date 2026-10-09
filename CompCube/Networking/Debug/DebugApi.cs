
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Server;
using CompCube.Interfaces;
using IPA.Utilities;

namespace CompCube.Server.Debug;

public class DebugApi : IApi
{
    public static readonly VotingMap[] Maps =
    [
        new("", "44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Tech), 
        new("", "44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.MidSpeed),
        new("", "44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Extreme),
        new("", "44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Speed),
        new("", "44d8d1c7c5821a7f1929542cab49c906c9e585e4", VotingMap.DifficultyType.ExpertPlus, VotingMap.Category.Accuracy)
    ];

    public static readonly UserStatistics DebugOpponent = new(
        "opponent",
        "1",
        null,
        "",
        null,
        false,
        2,
        1000,
        0, 0, 0, 0);

    public static readonly UserStatistics Self = new(
        "self",
        "0",
        null,
        "",
        null,
        false,
        1,
        1000,
         0, 0, 0, 0);
    
    public async Task<UserStatistics?> GetUserInfo(string id)
    {
        await Task.Delay(1000);
        return Self;
    }

    public Task<UserStatistics[]?> GetLeaderboardRange(int start, int range)
    {
        var info = new List<UserStatistics>()
        {
            DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self, DebugOpponent, Self
        };
        return Task.FromResult(info.ToArray());
    }

    public Task<UserStatistics[]?> GetAroundUser(string id)
    {
        return Task.FromResult(Array.Empty<UserStatistics>());
    }

    public Task<ServerStatus?> GetServerStatus()
    {
        return Task.FromResult(new ServerStatus([UnityGame.GameVersion.ToString()], [IPA.Loader.PluginManager.GetPluginFromId("CompCube").HVersion.ToString()],
            ServerState.State.Online));
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

    public async Task<byte[]?> DownloadBeatmap(string hash)
    {
        await Task.Delay(500);
        return [];
    }

    public Task<byte[]?> DownloadUserProfilePicture(CompCube_Models.Models.ClientData.UserInfo userInfo)
    {
        return Task.FromResult<byte[]?>([]);
    }
}
