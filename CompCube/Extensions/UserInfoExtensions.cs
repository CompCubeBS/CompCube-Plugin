using CompCube_Models.Models.ClientData;

namespace CompCube.Extensions;

public static class UserStatisticsExtensions
{
    public static string GetFormattedUserName(this UserStatistics userInfo)
    {
        if (userInfo.Flair == null) return userInfo.Username;
            
        var formatted = $"<color={userInfo.Flair.ColorCode}>{userInfo.Username}</color>";
        return formatted;
    }
}