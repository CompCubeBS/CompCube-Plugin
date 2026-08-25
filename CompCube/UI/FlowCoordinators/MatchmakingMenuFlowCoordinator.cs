using System.Collections;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Interfaces;
using CompCube.UI.BSML.Leaderboard;
using CompCube.UI.BSML.Menu;
using HMUI;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.EarlyLeaveWarning;
using CompCube.UI.ViewManagers;
using UnityEngine;
using Zenject;

namespace CompCube.UI.FlowCoordinators
{
    public class MatchmakingMenuFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly MatchFlowCoordinator _matchFlowCoordinator = null!;
        [Inject] private readonly InfoFlowCoordinator _infoFlowCoordinator = null!;
        
        [Inject] private readonly IServerListener _serverListener = null!;
        [Inject] private readonly MatchmakingMenuViewController _matchmakingMenuViewController = null!;

        [Inject] private readonly GameplaySetupViewManager _gameplaySetupViewManager = null!;
        [Inject] private readonly CompCubeLeaderboardViewController _leaderboardViewController = null!;
        [Inject] private readonly WarningModalViewController _warningModalViewController = null!;
        
        [Inject] private readonly DisconnectHandler _disconnectHandler = null!;
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            showBackButton = true;
            SetTitle("CompCube");
            ProvideInitialViewControllers(_matchmakingMenuViewController, rightScreenViewController: _leaderboardViewController, leftScreenViewController: _gameplaySetupViewManager.ManagedController);
        }

        private void OnMatchCreated(MatchCreatedPacket packet)
        {
            this.PresentFlowCoordinatorSynchronously(_matchFlowCoordinator);

            _matchFlowCoordinator.PopulateData(packet, () =>
            {
                DismissFlowCoordinator(_matchFlowCoordinator);
            });
        }

        public void Dispose()
        {
            _serverListener.OnMatchCreated -= OnMatchCreated;
            _infoFlowCoordinator.OnBackButtonPressed -= OnInfoFlowCoordinatorBackButtonPressed;
            _disconnectHandler.ShouldShowDisconnectScreen -= HandleShouldShowDisconnectScreen;
        }
        
        public void Initialize()
        {
            _matchmakingMenuViewController.SetButtonCallbacks(() =>
            {
                this.PresentFlowCoordinatorSynchronously(_infoFlowCoordinator);
            });
            
            _serverListener.OnMatchCreated += OnMatchCreated;
            _infoFlowCoordinator.OnBackButtonPressed += OnInfoFlowCoordinatorBackButtonPressed;
            _disconnectHandler.ShouldShowDisconnectScreen += HandleShouldShowDisconnectScreen;
        }

        private void HandleShouldShowDisconnectScreen(string reason)
        {
            _sharedCoroutineStarter.Run(HandleShouldShowDisconnectScreenCoroutine());
            return;
            
            IEnumerator HandleShouldShowDisconnectScreenCoroutine()
            {
                // this will break if the flow coordinator hierarchy ever gets more than 1 deeper after this flow coordinator
                if (childFlowCoordinator)
                    DismissFlowCoordinator(childFlowCoordinator);
                
                yield return new WaitUntil(() => isActivated && !isInTransition);
                
                _warningModalViewController.ParseOntoViewController(topViewController, $"Disconnected from server.\nReason: {reason}", _warningModalViewController.Hide);
            }
        }

        private void OnInfoFlowCoordinatorBackButtonPressed() => DismissFlowCoordinator(_infoFlowCoordinator);

        private void OnAboutButtonClicked()
        {
            this.PresentFlowCoordinatorSynchronously(_infoFlowCoordinator);
        }

        protected override void BackButtonWasPressed(ViewController viewController)
        {
            if (_serverListener.Connected)
            {
                _warningModalViewController.ParseOntoViewController(viewController, "Are you sure you want to leave the matchmaking queue?", () =>
                {
                    _serverListener.DisconnectAsync();
                    _mainFlowCoordinator.DismissFlowCoordinator(this);
                }, _warningModalViewController.Hide);
                return;
            }
                
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}  