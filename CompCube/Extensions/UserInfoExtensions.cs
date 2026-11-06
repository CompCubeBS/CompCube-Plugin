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

    public static string GetFormattedUserName(this UserProfileFull userInfo)
    {
        if (userInfo.Flairs.Length == 0) return userInfo.Username;
        
        var flair = userInfo.Flairs[0];
            
        var formatted = $"<color={flair.ColorCode}>{userInfo.Username}</color>";
        return formatted;
    }

    public static string GetFormattedRoles(this UserProfileFull profile) => string.Join(", ".FormatWithHtmlColor("#6F6F6F"), profile.Flairs.Select(i => $"{i.Name}".FormatWithHtmlColor(i.ColorCode)));
    
    public static string GetFormattedRoles(this UserProfile profile) => string.Join(", ".FormatWithHtmlColor("#6F6F6F"), profile.Flairs.Select(i => $"{i.Name}".FormatWithHtmlColor(i.ColorCode)));

}