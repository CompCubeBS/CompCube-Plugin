using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Map;
using CompCube.UI.BSML.Components.CustomMapList;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match;

[ViewDefinition("CompCube.UI.BSML.Match.WaitingForDiscardPhaseToFinishView.bsml")]
public class WaitingForDiscardPhaseToFinishViewController : BSMLAutomaticViewController
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    [UIValue("topText")] private string TopText { get; set; } = "These are your cards for this match!\nWhen it's your turn, you can play a card to go head-to-head against your opponent.";
    
    private CustomMapListController _customMapListController;

    [UIObject("opponentTextHorizontal")] private readonly GameObject _opponentTextHorizontal = null!;
    
    [UIAction("#post-parse")]
    private void PostParse()
    {
        _customMapListController = CustomMapListController.ParseOntoViewController(this, null, 0f, -10f);
    }

    public void PopulateData(VotingMap[] votingMaps, bool waitingForOpponent = true)
    {
        _sharedCoroutineStarter.Run(PopulateDataCoroutine());
        return;
        
        IEnumerator PopulateDataCoroutine()
        {
            yield return new WaitUntil(() => isActivated);
            yield return new WaitForEndOfFrame();
            
            _customMapListController.SetMaps(votingMaps, false);
        
            SetWaitingForOpponent(waitingForOpponent);
        }
    }

    public void SetWaitingForOpponent(bool waitingForOpponent)
    {
        _opponentTextHorizontal.SetActive(waitingForOpponent);
    }
}