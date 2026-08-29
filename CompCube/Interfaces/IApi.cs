using CompCube.Models;

namespace CompCube.Interfaces;

public interface IApi
{
    public Task<CompCube.Models.UserInfo?> GetUserInfo(string id);

    public Task<CompCube.Models.UserInfo[]?> GetLeaderboardRange(int start, int range);

    public Task<CompCube.Models.UserInfo[]?> GetAroundUser(string id);

    public Task<ServerStatus?> GetServerStatus();
    
    public Task<Queue[]?> GetQueues();

    public Task<string[]?> GetMapHashes();
    
    public Task<EventData[]?> GetEvents();

    public Task<byte[]?> DownloadBeatmap(string hash);
    
    public Task<byte[]?> DownloadUserProfilePicture(CompCube.Models.UserInfo userInfo);
}
