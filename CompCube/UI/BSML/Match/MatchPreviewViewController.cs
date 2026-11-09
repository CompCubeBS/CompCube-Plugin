using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.UI.BSML.Components.EnergyBar;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

[ViewDefinition("CompCube.UI.BSML.Match.MatchPreviewView.bsml")]
public class MatchPreviewViewController : BSMLAutomaticViewController
{
    [UIComponent("red-avatar")] private readonly Image _redAvatar = null!;
    [UIComponent("blue-avatar")] private readonly Image _blueAvatar = null!;

    [UIValue("red-avatar-source")] private string RedAvatarSource { get; set; }= string.Empty;
    [UIValue("blue-avatar-source")] private string BlueAvatarSource { get; set; }= string.Empty;

    [UIValue("red-name-text")] private string RedNameText { get; set; } = string.Empty;
    [UIValue("blue-name-text")] private string BlueNameText { get; set; } = string.Empty;
    
    [UIAction("#post-parse")]
    void PostParse()
    {
        // var redBar = CustomEnergyBar.InstantiateOntoViewController(this, new Vector3(-42.5f, 12.5f), false, new Color(1f, .4549f, .4235f));
        // redBar.SetEnergy(1.35f);
        //
        // var blueBar = CustomEnergyBar.InstantiateOntoViewController(this, new Vector3(42.5f, -12.5f), true, new Color(.565f, .835f, 1f));
        // blueBar.SetEnergy(1.35f);
        
        var material = Resources.FindObjectsOfTypeAll<Material>()
            .FirstOrDefault(m => m.name == "UINoGlowRoundEdge");
        
        _redAvatar.material = material;
        _blueAvatar.material = material;
    }
    
    public void PopulateData(UserProfile red, UserProfile blue)
    {
        RedAvatarSource = red.AvatarUrl;
        BlueAvatarSource = blue.AvatarUrl;
        
        RedNameText = red.GetFormattedUserName() + $" <size=50%>({red.CompetitiveStats.Elo} ELO)</size>";
        BlueNameText = $"<size=50%>({blue.CompetitiveStats.Elo} ELO) </size>" + blue.GetFormattedUserName();
    }
}