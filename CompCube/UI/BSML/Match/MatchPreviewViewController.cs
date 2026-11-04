using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using UnityEngine;
using UnityEngine.UI;

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
        var material = Resources.FindObjectsOfTypeAll<Material>()
            .FirstOrDefault(m => m.name == "UINoGlowRoundEdge");
        
        _redAvatar.material = material;
        _blueAvatar.material = material;
    }
    
    public void PopulateData(UserStatistics red, UserStatistics blue)
    {
        RedAvatarSource = red.AvatarUrl;
        BlueAvatarSource = blue.AvatarUrl;
        
        RedNameText = red.GetFormattedUserName();
        BlueNameText = blue.GetFormattedUserName();
    }
}