using CompCube_Models.Models.Map;
using HMUI;
using CompCube.Extensions;
using Zenject;

namespace CompCube.UI.ViewManagers;

public class StandardLevelDetailViewManager : ViewManager
{
    [Inject] private readonly StandardLevelDetailViewController _standardLevelDetailViewController = null!;
    
    public override ViewController ManagedController => _standardLevelDetailViewController;
    
    private Action<VotingMap>? _buttonPressedCallback;
    public VotingMap CurrentVotingMap { get; private set; }
    
    public void SetData(VotingMap votingMap, Action<VotingMap> buttonPressedCallback, string buttonText, bool buttonInteractable = true)
    {
        CurrentVotingMap = votingMap;
        
        _buttonPressedCallback = buttonPressedCallback;
        
        _standardLevelDetailViewController.SetData(
            votingMap.GetBeatmapLevel(), 
            true, 
            buttonText, 
            votingMap.GetBaseGameDifficultyTypeMask(), 
            votingMap.GetBeatmapLevel()?.beatmapBasicData.Keys
                .Select(i => i.characteristic)
                .Where(i => i.serializedName != "Standard")
                .ToArray()
            );
        ChangeButtonInteractability(buttonInteractable);
    }

    public void ChangeButtonInteractability(bool interactable)
    {
        _standardLevelDetailViewController._standardLevelDetailView.actionButton.interactable = interactable;
    }

    protected override void SetupManagedController()
    {
        _standardLevelDetailViewController._standardLevelDetailView.actionButton.onClick.AddListener(OnActionButtonPressed);
    }

    private void OnActionButtonPressed()
    {
        _buttonPressedCallback?.Invoke(CurrentVotingMap);
        _buttonPressedCallback = null;
    }

    protected override void ResetManagedController()
    {
        _standardLevelDetailViewController._standardLevelDetailView.actionButton.onClick.RemoveListener(OnActionButtonPressed);
    }
}