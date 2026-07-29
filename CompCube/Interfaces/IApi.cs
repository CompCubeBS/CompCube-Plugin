using CompCube_Models.Models.Events;
using CompCube_Models.Models.Server;
using JetBrains.Annotations;

namespace CompCube.Interfaces;

public interface IApi
{
    public Task<CompCube_Models.Models.ClientData.UserInfo?> GetUserInfo(string id);

    public Task<CompCube_Models.Models.ClientData.UserInfo[]?> GetLeaderboardRange(int start, int range);

    public Task<CompCube_Models.Models.ClientData.UserInfo[]?> GetAroundUser(string id);

    public Task<ServerStatus?> GetServerStatus();

    public Task<string[]?> GetMapHashes();
    
    public Task<EventData[]?> GetEvents();

    public Task<byte[]?> DownloadBeatmap(string hash);
    
    public Task<byte[]?> DownloadUserProfilePicture(CompCube_Models.Models.ClientData.UserInfo userInfo);
}