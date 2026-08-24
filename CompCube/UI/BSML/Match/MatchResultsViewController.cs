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
    
    /** Shows a win, loss or draw and keeps the server-provided completion reason visible. */
    public void PopulateData(string result, int eloChange, string? reason, Action continueButtonPressedCallback)
    {
        _continueButtonPressedCallback = continueButtonPressedCallback;
		var won = result == "win";
		var draw = result == "draw";
		TitleText = won ? "Victory!" : draw ? "Draw" : "Defeat...";
		TitleBgColor = won ? "#0000FF" : draw ? "#666666" : "#FF0000";
		var rating = draw
			? "No MMR change"
			: $"You {(won ? "gained" : "lost")}: {Math.Abs(eloChange).ToString().FormatWithHtmlColor(won ? "#90EE90" : "#FF7F7F")} MMR";
		MmrChangeText = string.IsNullOrWhiteSpace(reason) ? rating : $"{rating}\n{reason.Replace('_', ' ')}";
            
        NotifyPropertyChanged(null);
    }

    [UIAction("continueButtonClicked")]
    private void OnContinueButtonPressed()
    {
        _continueButtonPressedCallback?.Invoke();
        _continueButtonPressedCallback = null;
    }
}
