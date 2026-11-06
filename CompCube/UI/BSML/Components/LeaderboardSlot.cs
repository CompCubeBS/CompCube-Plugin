using BeatSaberMarkupLanguage.Attributes;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;

namespace CompCube.UI.BSML.Components;

public class LeaderboardSlot(UserProfile profile, bool isSelf)
{
    private const string OwnCellTextColor = "#00C0FF";

    public event Action<UserProfile> OnUserInfoButtonClicked;

    [UIValue("leaderboardCellColor")] private string LeaderboardCellColor { get; set; } = isSelf ? OwnCellTextColor : "white";

    [UIValue("rankText")] private string RankText { get; set; } = profile.CompetitiveStats.Elo + ".";
    [UIValue("playerNameText")] private string NameText { get; set; } = profile.GetFormattedUserName();
    [UIValue("mmrText")] private string MmrText { get; set; } = $"{profile.CompetitiveStats.Elo:N0} ELO".FormatWithHtmlColor(isSelf ? OwnCellTextColor : "white");

    [UIAction("profileButtonOnClick")]
    private void ProfileButtonOnClick() => OnUserInfoButtonClicked?.Invoke(profile);
}