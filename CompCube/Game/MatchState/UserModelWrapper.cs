using CompCube.Configuration;
using Zenject;

namespace CompCube.Game.MatchState;

public class UserModelWrapper
{
    [Inject] private readonly IPlatformUserModel _platformUserModel = null!;
    [Inject] private readonly PluginConfig _config = null!;

    public string UserId => _platformUserModel.GetUserInfo(CancellationToken.None).Result.platformUserId;
    
    public string UserName => _platformUserModel.GetUserInfo(CancellationToken.None).Result.userName;
}