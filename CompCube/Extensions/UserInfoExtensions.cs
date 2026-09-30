namespace CompCube.Extensions;

public static class UserInfoExtensions
{
    public static string GetFormattedUserName(this CompCube_Models.Models.ClientData.UserInfo userInfo)
    {
        if (userInfo.Badge == null) return userInfo.Username;
            
        var formatted = $"<color={userInfo.Badge.ColorCode}>{userInfo.Username}</color>";
        return !userInfo.Badge.Bold ? formatted : $"<b>{formatted}</b>";
    }
}