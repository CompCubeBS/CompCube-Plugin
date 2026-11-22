using System.Collections;
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

    [Inject] private readonly GameplaySetupViewManager _gameplaySetupViewManager = null!;
    [Inject] private readonly PlatformLeaderboardViewController _platformLeaderboardViewController = null!;
    
    [Inject] private readonly MatchPreviewViewController _matchPreviewViewController = null!;
    [Inject] private readonly MapPickingViewController _mapPickingViewController = null!;
    [Inject] private readonly WaitingForMatchToStartViewController _waitingForMatchToStartViewController = null!;

    [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
    
    [Inject] private readonly SoundEffectManager _soundEffectManager = null!;
    [Inject] private readonly FireworksManager _fireworksManager = null!;

    [Inject] private readonly SiraLog _siraLog = null!;
    
    
    
    public Action? OnMatchConcluded { get; set; } = null;
    
    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _serverListener.OnPlayerSelectedMap += HandlePlayerSelectedMap;
        
        _mapPickingViewController.OnBeatmapCardSelected += ShowLeaderboardOrHide;
        
        ProvideInitialViewControllers(_matchPreviewViewController, _gameplaySetupViewManager.ManagedController);
        showBackButton = true;
        SetTitle("Match Room");
        
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

    private void HandlePlayerSelectedMap(PlayerSelectedMapPacket packet) => ShowMapPreview(packet.Map);

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

    private void ShowMapPreview(CCBeatmapData map)
    {
        ShowLeaderboardOrHide(map);
        _soundEffectManager.PlayGongSoundEffect();
        this.ReplaceViewControllerSynchronously(_waitingForMatchToStartViewController);
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _mapPickingViewController.OnBeatmapCardSelected -= ShowLeaderboardOrHide;
        
        _serverListener.OnPlayerSelectedMap -= HandlePlayerSelectedMap;
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