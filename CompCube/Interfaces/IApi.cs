using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Events;
using CompCube_Models.Models.Server;

namespace CompCube.Interfaces;

public interface IApi
{
    public Task<UserStatistics?> GetUserInfo(string id);

    public Task<UserStatistics[]?> GetLeaderboardRange(int start, int range);

    public Task<UserStatistics[]?> GetAroundUser(string id);

    public Task<ServerStatus?> GetServerStatus();

    public Task<string[]?> GetMapHashes();

    public Task<Queue[]?> GetQueues();

    public Task<byte[]?> DownloadBeatmap(string hash);
}
