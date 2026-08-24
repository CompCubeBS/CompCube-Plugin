using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using SiraUtil.Logging;
using Zenject;

namespace CompCube.UI.BSML.Match;

[ViewDefinition("CompCube.UI.BSML.Match.MatchResultsView.bsml")]
public class MatchResultsViewController : BSMLAutomaticViewController
{
    [Inject] private readonly MatchStateManager _stateManager = null!;
    [Inject] private readonly SiraLog _siraLog = null!;
    
    [UIValue("titleBgColor")] private string TitleBgColor { get; set; } = "#0000FF";
    [UIValue("titleText")] private string TitleText { get; set; } = "You Win";

    [UIValue("mmrChangeText")] private string MmrChangeText { get; set; } = "";
    
    private Action? _continueButtonPressedCallback = null;
    
    public void PopulateData(bool won, int eloChange, Action continueButtonPressedCallback)
    {
        _continueButtonPressedCallback = continueButtonPressedCallback;
        TitleText = won ? "Victory!" : "Defeat...";
        TitleBgColor = won ? "#0000FF" : "#FF0000";

        MmrChangeText =
            $"You {(won ? "gained" : "lost")}: {eloChange.ToString().FormatWithHtmlColor(won ? "#90EE90" : "#FF7F7F")} ELO";
            
        NotifyPropertyChanged(null);
    }

    [UIAction("continueButtonClicked")]
    private void OnContinueButtonPressed()
    {
        _continueButtonPressedCallback?.Invoke();
        _continueButtonPressedCallback = null;
    }
}