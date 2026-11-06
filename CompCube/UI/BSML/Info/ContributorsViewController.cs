using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.Interfaces;
using HMUI;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.UI.BSML.Info;

[ViewDefinition("CompCube.UI.BSML.Info.ContributorsView.bsml")]
public class ContributorsViewController : BSMLAutomaticViewController
{
    [Inject] private readonly IApi _api = null!;
    [Inject] private readonly SiraLog _siraLog = null!;
    
    [UIValue("contributors-tab-items")] private List<object> _contributorsTabItems = [new ContributorsTabItem("Admin", "role:admin"), new ContributorsTabItem("Developers", "role:dev")];
    [UIComponent("contributors-tab-selector")] private readonly TabSelector _tabSelector = null!;

    [UIValue("contributor-profiles")] private List<object> _contributorProfiles = [];
    
    [UIAction("#post-parse")]
    void PostParse()
    {
        _tabSelector.TextSegmentedControl.ReloadData();
        _tabSelector.Refresh();
    }
    
    [UIAction("on-contributors-tab-selector-cell-selected")]
    async void HandleContributorsTabSelectorCellSelected(SegmentedControl _, int idx)
    {
        try
        {
            var users = await _api.GetContributors(((ContributorsTabItem)_contributorsTabItems[idx]).Role);

            _contributorProfiles = users.Select(i => (object)new ContributorProfile(i)).ToList();
            
            NotifyPropertyChanged(null);
        }
        catch (Exception e)
        {
            _siraLog.Error("Failed to fetch contributor profiles: " + e);
        }
    }
}

public class ContributorProfile(UserProfileFull userProfile)
{
    [UIValue("avatar-source")] private string AvatarSource => userProfile.AvatarUrl;

    [UIValue("contributor-text")]
    private string ContributorText =>
        $"{userProfile.GetFormattedUserName()}\n<size=50%>{userProfile.GetFormattedRoles()}</size>";
}

public class ContributorsTabItem(string tabName, string contributorsRole)
{
    [UIValue("tab-name")]
    public string TabName => tabName;
    
    [UIValue("role")]
    public string Role => contributorsRole;
}