using CompCube_Models.Models.ClientData;

namespace CompCube.Extensions;

public static class UserStatisticsExtensions
{
    public static string GetFormattedUserName(this UserProfile userInfo)
    {
        if (userInfo.Flairs.Length == 0) return userInfo.UserName;
        
        var flair = userInfo.Flairs[0];
            
        var formatted = $"<color={flair.ColorCode}>{userInfo.UserName}</color>";
        return formatted;
    }
}