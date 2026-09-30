using BeatSaberMarkupLanguage.Attributes;
using CompCube.Extensions;

namespace CompCube.UI.BSML.Components;

public class LeaderboardSlot(CompCube_Models.Models.ClientData.UserInfo userInfo, bool isSelf)
{
    private const string OwnCellTextColor = "#00C0FF";

    public event Action<CompCube_Models.Models.ClientData.UserInfo> OnUserInfoButtonClicked;

    [UIValue("leaderboardCellColor")] private string LeaderboardCellColor { get; set; } = isSelf ? OwnCellTextColor : "white";

    [UIValue("rankText")] private string RankText { get; set; } = userInfo.Rank + ".";
    [UIValue("playerNameText")] private string NameText { get; set; } = userInfo.GetFormattedUserName();
    [UIValue("mmrText")] private string MmrText { get; set; } = $"{userInfo.Mmr:N0} MMR".FormatWithHtmlColor(isSelf ? OwnCellTextColor : "white");

    [UIAction("profileButtonOnClick")]
    private void ProfileButtonOnClick() => OnUserInfoButtonClicked?.Invoke(userInfo);
}