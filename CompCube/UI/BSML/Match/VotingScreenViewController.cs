using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.ServerPackets;
using HMUI;
using JetBrains.Annotations;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.Components.CustomMapList;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match;

[ViewDefinition("CompCube.UI.BSML.Match.VotingScreenView.bsml")]
public class VotingScreenViewController : BSMLAutomaticViewController
{
    [Inject] private readonly SiraLog _log = null!;
    [Inject] private readonly MatchStateManager _matchStateManager = null!;
    
    public event Action<VotingMap>? MapSelected;
    private Action? _ranOutOfTimeCallback = null;

    [UIComponent("voteStatusText")] private readonly TextMeshProUGUI _voteStatusText = null!;
    [UIValue("timerText")] private string TimerText { get; set; } = "";
    [UIValue("votingText")] private string VotingText { get; set; } = "";
    
    private Action? _activationCallback = null;
    
    private Action? _skipButtonPressedCallback = null;
    
    [UIValue("showSkipButton")] private bool ShowSkipButton => _skipButtonPressedCallback != null;
    
    private CustomMapListController _customMapListController;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        
        _activationCallback?.Invoke();
        _activationCallback = null;
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        
        StopCountdown();
    }

    [UIAction("#post-parse")]
    private void PostParse()
    {
        _customMapListController = CustomMapListController.ParseOntoViewController(this, (map) => MapSelected?.Invoke(map), 0, -10);
    }
    
    public void EmptyMapList() => _customMapListController.SetMaps([]);

    public void StopCountdown() => _ranOutOfTimeCallback = null;

    [UIAction("skipButtonClicked")]
    private void SkipButtonClicked()
    {
        _skipButtonPressedCallback?.Invoke();
        _skipButtonPressedCallback = null;
    }

    public void ClearSelection()
    {
        _customMapListController.ClearSelection();
    }

    public void RemoveMapFromList(VotingMap map)
    {
        _customMapListController.SetMaps(_customMapListController.MapsInList.Where(i => i != map).ToArray());
    }

    public void PopulateData(string text, VotingMap[] maps, int waitTime, Action? skipButtonPressedCallback = null, Action? timerRanOutCallback = null)
    {
        _log.Notice("Populating maps");
        StartCoroutine(CountDown());
        
        _customMapListController.SetMaps(maps);
        
        _ranOutOfTimeCallback = timerRanOutCallback;
        _skipButtonPressedCallback = skipButtonPressedCallback;
        
        VotingText = text;
        NotifyPropertyChanged(null);
        return;

        IEnumerator CountDown()
        {
            var countdownFinishTime = DateTime.Now.AddSeconds(waitTime);
            
            _log.Notice("Counting down");
            while (true)
            {
                if (_ranOutOfTimeCallback == null)
                    yield return null;
                
                var remaining = countdownFinishTime - DateTime.Now;
                if (remaining.TotalSeconds <= 0)
                    break;

                TimerText = $"{remaining.TotalSeconds.ToString("00", CultureInfo.InvariantCulture)}.{Mathf.CeilToInt(remaining.Milliseconds).ToString("D", CultureInfo.InvariantCulture)}";
                NotifyPropertyChanged(nameof(TimerText));

                yield return null;
            }
            _ranOutOfTimeCallback?.Invoke();
            _ranOutOfTimeCallback = null;
        }
    }
}