using System.Collections;
using System.Windows.Forms;
using BeatSaberMarkupLanguage;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Game;
using CompCube.Interfaces;
using CompCube.UI.BSML.Match;
using CompCube.UI.Sound;
using CompCube.UI.ViewManagers;
using HMUI;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.EarlyLeaveWarning;
using CompCube.UI.BSML.Modal;
using SiraUtil.Logging;
using SongCore;
using UnityEngine;
using Zenject;

namespace CompCube.UI.FlowCoordinators;

public class MatchFlowCoordinator : FlowCoordinator
{
    [Inject] private readonly IServerListener _serverListener = null!;

    [Inject] private readonly MatchStateManager _matchStateManager = null!;

    [Inject] private readonly GameplaySetupViewManager _gameplaySetupViewManager = null!;
    [Inject] private readonly PlatformLeaderboardViewController _platformLeaderboardViewController = null!;
    
    [Inject] private readonly MatchPreviewViewController _matchPreviewViewController = null!;
    [Inject] private readonly MapPickingViewController _mapPickingViewController = null!;
    [Inject] private readonly WaitingForMatchToStartViewController _waitingForMatchToStartViewController = null!;
    [Inject] private readonly WaitingViewController _waitingViewController = null!;
    [Inject] private readonly RoundResultsViewController _roundResultsViewController = null!;
    [Inject] private readonly MatchResultsViewController _matchResultsViewController = null!;

    [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
    
    [Inject] private readonly SoundEffectManager _soundEffectManager = null!;
    [Inject] private readonly FireworksManager _fireworksManager = null!;
    [Inject] private readonly TransitionToLevelManager _transitionToLevelManager = null!;

    [Inject] private readonly SiraLog _siraLog = null!;
    
    
    
    public Action? OnMatchConcluded { get; set; } = null;
    
    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _serverListener.OnPlayerSelectedMap += HandlePlayerSelectedMap;
        _serverListener.OnRoundResults += HandleRoundResults;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
        _serverListener.OnMatchFinished += HandleMatchResults;
        _serverListener.OnReplayRequested += HandleReplayRequested;
        
        _mapPickingViewController.OnBeatmapCardSelected += ShowLeaderboardOrHide;
        
        ProvideInitialViewControllers(_matchPreviewViewController);
        showBackButton = true;
        SetTitle("Match Room");

        _matchResultsViewController.OnContinueButtonPressedCallback = OnMatchConcluded;

        if (!addedToHierarchy)
            return;
        
        _soundEffectManager.PlayGongSoundEffect();
        
        StartCoroutine(WaitForTimeAndPresentCardView());
        return;

        IEnumerator WaitForTimeAndPresentCardView()
        {
            yield return new WaitForSeconds(5f);
            
            this.ReplaceViewControllerSynchronously(_mapPickingViewController);
            
            this.SetBottomScreenViewController(_bottomScreenMatchStateViewController, ViewController.AnimationType.In);
        }
    }

    private void HandleReplayRequested(PlayerDidRequestReplayPacket packet)
    {
        ShowMapPreviewAndStartMatch(packet.MapToReplay!);
    }

    private void HandleMatchResults(MatchFinishedPacket packet)
    {
        this.ReplaceViewControllerSynchronously(_matchResultsViewController);
        SetBottomScreenViewController(null, ViewController.AnimationType.Out);
        SetLeftScreenViewController(null, ViewController.AnimationType.Out);
        SetRightScreenViewController(null, ViewController.AnimationType.Out);
    }

    private void HandlePickPhaseStarted(StartPickPhasePacket packet)
    {
        this.ReplaceViewControllerSynchronously(_mapPickingViewController);
    }

    private void HandleRoundResults(RoundResultsPacket packet)
    {
        this.ReplaceViewControllerSynchronously(_roundResultsViewController);
    }

    private void HandlePlayerSelectedMap(PlayerSelectedMapPacket packet)
    {
        _soundEffectManager.CrossfadeToDefault();
        _bottomScreenMatchStateViewController.Timer = null;
        ShowMapPreviewAndStartMatch(packet.Map);
    }

    private void ShowLeaderboardOrHide(CCBeatmapData? beatmap)
    {
        StartCoroutine(ShowLeaderboardCoroutine());
        return;
        
        IEnumerator ShowLeaderboardCoroutine()
        {
            if (beatmap is null)
            {
                SetRightScreenViewController(null, ViewController.AnimationType.Out);
                yield return null;
            }
            
            _platformLeaderboardViewController.SetData(beatmap!.GetBeatmapKey());
            yield return new WaitForEndOfFrame();
            SetRightScreenViewController(_platformLeaderboardViewController, ViewController.AnimationType.In);
        }
    }

    private void ShowMapPreviewAndStartMatch(CCBeatmapData map)
    {
        ShowLeaderboardOrHide(map);
        _soundEffectManager.PlayGongSoundEffect();
        this.ReplaceViewControllerSynchronously(_waitingForMatchToStartViewController);
        SetLeftScreenViewController(_gameplaySetupViewManager.ManagedController, ViewController.AnimationType.In);

        StartCoroutine(WaitForSecondsAndStartMatch());
        return;
        
        IEnumerator WaitForSecondsAndStartMatch()
        {
            yield return new WaitForSeconds(15f);
            
            _transitionToLevelManager.StartLevel(map, DateTime.Now.AddSeconds(15), _gameplaySetupViewManager.ProMode,
                (results, transitionSetupDataSo) =>
                {
                    _serverListener.SubmitScoreAsync(new Score(results.multipliedScore, (float) results.multipliedScore / ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transitionSetupDataSo.transformedBeatmapData), 
                        results.gameplayModifiers.proMode, results.badCutsCount + results.missedCount + results.notGoodCount, results.fullCombo));
                    
                    this.ReplaceViewControllerSynchronously(_waitingViewController, true);
                    ShowLeaderboardOrHide(null);
                    SetLeftScreenViewController(null, ViewController.AnimationType.None);
                    _waitingViewController.SetText($"Waiting for {_matchStateManager.Opponent.GetFormattedUserName()} to submit a score!");
                });
        }
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _mapPickingViewController.OnBeatmapCardSelected -= ShowLeaderboardOrHide;
        
        _serverListener.OnPlayerSelectedMap -= HandlePlayerSelectedMap;
        _serverListener.OnRoundResults -= HandleRoundResults;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
        _serverListener.OnMatchFinished -= HandleMatchResults;
        _serverListener.OnReplayRequested -= HandleReplayRequested;
    }

    protected override void BackButtonWasPressed(ViewController viewController)
    {
        YesOrNoDialogModal.CreateOnTransform(viewController.transform, 
            "Are you sure you want to leave the match early? Leaving the match early could result in penalties",
            true,
            async void () =>
            {
                try
                {
                    _soundEffectManager.CrossfadeToDefault();
                    OnMatchConcluded?.Invoke();
                    _fireworksManager.StopFireworks();
                    await _serverListener.DisconnectAsync();
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            },
            () => { }
        );
    }
}