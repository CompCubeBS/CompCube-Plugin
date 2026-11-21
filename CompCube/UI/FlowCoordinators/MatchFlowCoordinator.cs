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
    
    [Inject] private readonly MatchPreviewViewController _matchPreviewViewController = null!;
    [Inject] private readonly MapPickingViewController _mapPickingViewController = null!;

    [Inject] private readonly SoundEffectManager _soundEffectManager = null!;
    [Inject] private readonly FireworksManager _fireworksManager = null!;

    [Inject] private readonly SiraLog _siraLog = null!;
    
    
    
    public Action? OnMatchConcluded { get; set; } = null;
    
    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        ProvideInitialViewControllers(_matchPreviewViewController, _gameplaySetupViewManager.ManagedController);
        showBackButton = true;
        SetTitle("Match Room");

        StartCoroutine(WaitForTimeAndPresentCardView());
        return;

        IEnumerator WaitForTimeAndPresentCardView()
        {
            yield return new WaitForSeconds(5f);
            
            this.ReplaceViewControllerSynchronously(_mapPickingViewController);
        }
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