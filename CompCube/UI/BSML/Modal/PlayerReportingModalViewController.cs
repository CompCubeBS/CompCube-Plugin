using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Interfaces;
using HMUI;
using Zenject;

namespace CompCube.UI.BSML.PlayerReporting;

[ViewDefinition("CompCube.UI.BSML.Modal.PlayerReportingView.bsml")]
public class PlayerReportingModalViewController : BSMLAutomaticViewController
{
    [Inject] private readonly IApi _api = null!;
    
    public void ParseOntoViewController(ViewController viewController, string? matchGuid, Action? callback = null)
    {
        
    }
}