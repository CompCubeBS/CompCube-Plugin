using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Events;
using CompCube_Models.Models.Server;

namespace CompCube.Interfaces;

public interface IApi
{
    public Task<UserProfileFull?> GetUserProfile(string id);

    public Task<UserProfile[]?> GetLeaderboardRange(int start, int range);

    public Task<UserProfile[]?> GetAroundUser(string id);

    public Task<ServerStatus?> GetServerStatus();
    
    public Task<UserProfileFull[]> GetContributors(string role);

    public Task<string[]?> GetMapHashes();

    public Task<Queue[]?> GetQueues();

    public Task<byte[]?> DownloadBeatmap(string hash);
}
