using System.Collections;
using BeatSaberMarkupLanguage;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube.Game;
using CompCube.Interfaces;
using CompCube.UI.BSML.Match;
using CompCube.UI.Sound;
using CompCube.UI.ViewManagers;
using HMUI;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.EarlyLeaveWarning;
using CompCube.UI.BSML.Match.Modal;
using SiraUtil.Logging;
using SongCore;
using UnityEngine;
using Zenject;

namespace CompCube.UI.FlowCoordinators
{
    public class MatchFlowCoordinator : FlowCoordinator
    {
        [Inject] private readonly VotingScreenViewController _votingScreenViewController = null!;
        [Inject] private readonly WaitingForMatchToStartViewController _waitingForMatchToStartViewController = null!;
        [Inject] private readonly WaitingViewController _waitingViewController = null!;
        [Inject] private readonly RoundResultsViewController _roundResultsViewController = null!;
        [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
        [Inject] private readonly MatchResultsViewController _matchResultsViewController = null!;
        [Inject] private readonly WaitingForDiscardPhaseToFinishViewController _waitingForDiscardPhaseToFinishViewController = null!;
        [Inject] private readonly WarningModalViewController _warningModalViewController = null!;
        
        [Inject] private readonly PhasePopupViewController _phasePopupViewController = null!;
         
        [Inject] private readonly IServerListener _serverListener = null!;
        [Inject] private readonly TransitionToLevelManager _transitionToLevelManager = null!;
        [Inject] private readonly MatchStateManager _matchStateManager = null!;
        [Inject] private readonly MatchBeatmapManager _matchBeatmapManager = null!;
        
        [Inject] private readonly SiraLog _siraLog = null!;
        
        [Inject] private readonly StandardLevelDetailViewManager _standardLevelDetailViewManager = null!;
        [Inject] private readonly GameplaySetupViewManager _gameplaySetupViewManager = null!;
        
        [Inject] private readonly SoundEffectManager _soundEffectManager = null!;
        [Inject] private readonly BeatmapDownloader _beatmapDownloader = null!;
        
        [Inject] private readonly PlatformLeaderboardViewController _platformLeaderboardViewController = null!;

        private NavigationController _votingScreenNavigationController;

        private Action? _onMatchFinishedCallback = null;
        
        private bool _roundResultsAnimationInProgress = false;

        private bool _hasShownFinalCardsToPlayer = false;

        public void PopulateData(MatchCreatedPacket packet, Action? onMatchFinishedCallback)
        {
            _bottomScreenMatchStateViewController.PopulateData(packet.Red, packet.Blue);
            _bottomScreenMatchStateViewController.SetStatus("Discard Phase");
            _bottomScreenMatchStateViewController.UpdateHealth(_matchStateManager.RedHealth, _matchStateManager.BlueHealth);
            _bottomScreenMatchStateViewController.UpdateMultiplier(1f);

            StartCoroutine(WaitForVotingScreenToPresent());
            
            _onMatchFinishedCallback = onMatchFinishedCallback;
            return;
            
            IEnumerator WaitForVotingScreenToPresent()
            {
                yield return new WaitUntil(() => !isInTransition && isActivated);
                
                yield return new WaitUntil(() => !_votingScreenViewController.isInTransition && _votingScreenViewController.isActivated);
                
                DownloadingBeatmapsModal.DownloadLevelsAndParseModalOntoGameObject(_votingScreenViewController,
                    packet.InitialMaps.Select(i => i.Hash).ToArray(), () => StartCoroutine(PopulateDataCoroutine()));
            }

            IEnumerator PopulateDataCoroutine()
            {
                yield return new WaitUntil(() => _votingScreenViewController.isActivated);
                
                _votingScreenViewController.PopulateData("Discard up to two cards to be replaced!", packet.InitialMaps, 30, HandleSkippingDiscardPhase, HandleVotingScreenTimerRanOutDuringDiscardPhase);
                StartCoroutine(ShowPhaseChangeModal("Discard Phase", ""));
            }
        }

        private void HandleSkippingDiscardPhase()
        {
            HideStandardLevelDetailControllerIfPresent();
            HideLeaderboard();
            _matchBeatmapManager.SkipDiscardingMaps();
        }

        private IEnumerator ShowPhaseChangeModal(string topText, string subText)
        {
            yield return new WaitUntil(() => !isInTransition && !topViewController.isInTransition);
                
            _phasePopupViewController.ParseOntoObject(topViewController.gameObject, topText, subText);
        }
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Match Room");
            showBackButton = true;
            
            _votingScreenNavigationController = BeatSaberUI.CreateViewController<NavigationController>();
            
            ProvideInitialViewControllers(_votingScreenNavigationController, _gameplaySetupViewManager.ManagedController, bottomScreenViewController: _bottomScreenMatchStateViewController);
            _votingScreenNavigationController.PushViewController(_votingScreenViewController, null);
            
            _votingScreenViewController.MapSelected += HandleVotingScreenMapSelected;
            _serverListener.OnPickPhaseStarted += OnPickPhaseStarted;
            _serverListener.OnRoundResults += HandleRoundResults;
            _serverListener.OnPlayerSelectedMap += HandleOpponentSelectedMap;
            _matchBeatmapManager.CanNoLongerDiscardMaps += HandleCanNoLongerDiscardMaps;
            _serverListener.OnMatchFinished += HandleMatchFinished;
            _serverListener.OnCardsUpdated += HandleCardsUpdated;
        }

        private void HandleCardsUpdated(UpdateCardsPacket packet)
        {
            DownloadingBeatmapsModal.DownloadLevelsAndParseModalOntoGameObject(_waitingForDiscardPhaseToFinishViewController,
                packet.Maps.Select(i => i.Hash).ToArray(), () => StartCoroutine(PopulateDataCoroutine()));
            return;

            IEnumerator PopulateDataCoroutine()
            {
                this.ReplaceViewControllerSynchronously(_waitingForDiscardPhaseToFinishViewController); 
                yield return new WaitUntil(() => Loader.AreSongsLoaded);
                _waitingForDiscardPhaseToFinishViewController.PopulateData(packet.Maps);
            }
        }

        private void HandleMatchFinished(MatchFinishedPacket packet)
        {
            StartCoroutine(HandleMatchFinishedCoroutine());
            return;
            
            IEnumerator HandleMatchFinishedCoroutine()
            {
                yield return new WaitUntil(() => !_roundResultsAnimationInProgress);
                
                this.ReplaceViewControllerSynchronously(_matchResultsViewController);
                _matchResultsViewController.PopulateData(packet.Won, packet.EloChange, () => _onMatchFinishedCallback?.Invoke());
                
                SetBottomScreenViewController(null, ViewController.AnimationType.Out);
                SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                
                _serverListener.DisconnectAsync();
            }
        }

        private async void HandleCanNoLongerDiscardMaps()
        {
            try
            {
                await _serverListener.SendPacketAsync(new DiscardMapsPacket(_matchBeatmapManager.DiscardedMaps.ToArray()));
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }

        private void HandleOpponentSelectedMap(PlayerSelectedMapPacket packet)
        {
            StartCoroutine(HandleOpponentSelectedMapCoroutine());
            
            IEnumerator HandleOpponentSelectedMapCoroutine()
            {
                yield return new WaitForEndOfFrame();
                
                DownloadingBeatmapsModal.DownloadLevelsAndParseModalOntoGameObject(_waitingViewController, 
                    [packet.Map.Hash], ()=> StartCoroutine(ShowMapPreviewViewAndStartMatch(packet.Map)));
            }
        }

        private void HandleRoundResults(RoundResultsPacket results)
        {
            StartCoroutine(HandleRoundResultsCoroutine());

            return;
            
            IEnumerator HandleRoundResultsCoroutine()
            {
                _roundResultsAnimationInProgress = true;
                this.ReplaceViewControllerSynchronously(_roundResultsViewController);
                
                _roundResultsViewController.PopulateData(results, _matchStateManager.DamageMultiplier);
                
                yield return new WaitForSeconds(3f);

                _bottomScreenMatchStateViewController.UpdateHealth(results.RedHealth, results.BlueHealth);
                
                yield return new WaitForSeconds(3f);

                _roundResultsAnimationInProgress = false;
            }
        }

        private void OnPickPhaseStarted(StartPickPhasePacket packet)
        {
            StartCoroutine(OnPickPhaseStartedCoroutine());
            return;

            IEnumerator OnPickPhaseStartedCoroutine()
            {
                yield return new WaitUntil(() => !_roundResultsAnimationInProgress);
                
                _bottomScreenMatchStateViewController.UpdateRound(_matchStateManager.CurrentRound);
                _bottomScreenMatchStateViewController.UpdateMultiplier(packet.NewMultiplier);

                if (!_hasShownFinalCardsToPlayer)
                {
                    yield return ShowFinalCardsToPlayerCoroutine();
                }
                
                if (packet.IsOwnPick)
                {
                    yield return PickMapCoroutine();
                    yield break;
                }

                yield return WaitForMapToBePickedCoroutine();
            }

            IEnumerator ShowFinalCardsToPlayerCoroutine()
            {
                DownloadingBeatmapsModal.DownloadLevelsAndParseModalOntoGameObject(topViewController, packet.AvailableMaps.Select(i => i.Hash).ToArray());
                yield return new WaitUntil(() => !_beatmapDownloader.IsDownloadingBeatmaps);
                
                this.ReplaceViewControllerSynchronously(_waitingForDiscardPhaseToFinishViewController);
                _waitingForDiscardPhaseToFinishViewController.PopulateData(packet.AvailableMaps, false);

                yield return new WaitForSeconds(4f);
                
                _hasShownFinalCardsToPlayer = true;
            }
            
            IEnumerator PickMapCoroutine()
            {
                yield return new WaitUntil(() => !_waitingViewController.isInTransition);
                
                this.ReplaceViewControllerSynchronously(_votingScreenNavigationController);
                
                yield return new WaitUntil(() => _votingScreenViewController.isActivated);
                
                _votingScreenViewController.PopulateData("Pick a card to play against your opponent!", packet.AvailableMaps, 30, null, HandleVotingScreenTimerRanOutDuringPickPhase);

                yield return new WaitUntil(() => !_votingScreenViewController.isInTransition);  
                
                yield return ShowPhaseChangeModal("Pick Phase", $"{_matchStateManager.Self.GetFormattedUserName()}'s Pick");
            }

            IEnumerator WaitForMapToBePickedCoroutine()
            {
                yield return new WaitUntil(() => !_standardLevelDetailViewManager.ManagedController.isActivated);
                
                this.ReplaceViewControllerSynchronously(_waitingViewController);
                _waitingViewController.SetText($"Waiting for {_matchStateManager.Opponent.GetFormattedUserName()} to pick a map...");

                yield return new WaitUntil(() => _waitingViewController.isActivated);
                
                yield return ShowPhaseChangeModal("Pick Phase", $"{_matchStateManager.Opponent.GetFormattedUserName()}'s Pick");
            }
        }

        private void HandleVotingScreenTimerRanOutDuringPickPhase()
        {
            HideStandardLevelDetailControllerIfPresent();
            HandleStandardLevelDetailButtonPressed(_matchBeatmapManager.AvailablePicks[0]);
        }

        private void HandleVotingScreenTimerRanOutDuringDiscardPhase()
        {
            HideStandardLevelDetailControllerIfPresent();
            _matchBeatmapManager.SkipDiscardingMaps();
        }

        private void HandleVotingScreenMapSelected(VotingMap votingMap)
        {
            if (!_standardLevelDetailViewManager.ManagedController.isActivated)
                _votingScreenNavigationController.PushViewController(_standardLevelDetailViewManager.ManagedController,
                    () =>
                    {
                        _standardLevelDetailViewManager.ManagedController.transform.position = new Vector3(1.4f,
                            _standardLevelDetailViewManager.ManagedController.transform.position.y, 
                            _standardLevelDetailViewManager.ManagedController.transform.position.z);
                    });
            
            _standardLevelDetailViewManager.SetData(votingMap, HandleStandardLevelDetailButtonPressed, _matchBeatmapManager.InDiscardPhase ? "Discard" : "Select");
            ShowLeaderboard(votingMap);

            var beatmap = votingMap.GetBeatmapLevel();

            if (beatmap == null)
                return;
            
            _soundEffectManager.PlayBeatmapLevelPreview(beatmap);
        }

        private async void HandleStandardLevelDetailButtonPressed(VotingMap votingMap)
        {
            try
            {
                HideStandardLevelDetailControllerIfPresent();
                _votingScreenViewController.ClearSelection();
                _votingScreenViewController.RemoveMapFromList(votingMap);
                HideLeaderboard();
                
                if (_matchBeatmapManager.InDiscardPhase)
                {
                    _matchBeatmapManager.DiscardMap(votingMap);
                    return;
                }
                
                StartCoroutine(ShowMapPreviewViewAndStartMatch(votingMap));
                await _serverListener.SendPacketAsync(new MapSelectionPacket(votingMap));
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }

        # region Leaderboard Functions
        private void ShowLeaderboard(VotingMap votingMap)
        {
            StartCoroutine(ShowLeaderboardCoroutine());
            return;
            
            IEnumerator ShowLeaderboardCoroutine()
            {
                _platformLeaderboardViewController.SetData(votingMap.GetBeatmapKey());
                yield return new WaitForEndOfFrame();
                SetRightScreenViewController(_platformLeaderboardViewController, ViewController.AnimationType.In);
            }
        }

        private void HideLeaderboard(bool immediately = false)
        {
            SetRightScreenViewController(null, immediately ? ViewController.AnimationType.None : ViewController.AnimationType.Out);
        }

        #endregion
        private IEnumerator ShowMapPreviewViewAndStartMatch(VotingMap map)
        {
            yield return new WaitForEndOfFrame();
            
            DownloadingBeatmapsModal.DownloadLevelsAndParseModalOntoGameObject(topViewController, [map.Hash], () => StartCoroutine(WaitForSecondsAndStartMatch()));
            yield break;
            
            IEnumerator WaitForSecondsAndStartMatch()
            {
                ShowLeaderboard(map);
                _soundEffectManager.PlayGongSoundEffect();
                this.ReplaceViewControllerSynchronously(_waitingForMatchToStartViewController);
                
                yield return new WaitUntil(() => _waitingForMatchToStartViewController.isActivated);
                
                _waitingForMatchToStartViewController.PopulateData(map, DateTime.Now.AddSeconds(15));
                
                yield return new WaitForSeconds(15);

                if (!isActivated)
                    yield break;
                
                _transitionToLevelManager.StartLevel(map, DateTime.Now.AddSeconds(10), _gameplaySetupViewManager.ProMode, async void (results, transitionSetupDataSo) =>
                {
                    try
                    {
                        this.ReplaceViewControllerSynchronously(_waitingViewController, true);
                        _waitingViewController.SetText($"Waiting for {_matchStateManager.Opponent.GetFormattedUserName()} to submit a score...");
                        
                        HideLeaderboard(true);
                        await _serverListener.SendPacketAsync(new ScoreSubmissionPacket(results.multipliedScore,
                            ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transitionSetupDataSo.transformedBeatmapData),
                            results.gameplayModifiers.proMode, results.notGoodCount, results.fullCombo));
                    }
                    catch (Exception e)
                    {
                        _siraLog.Error(e);
                    }
                });
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _votingScreenViewController.MapSelected -= HandleVotingScreenMapSelected;
            _serverListener.OnPickPhaseStarted -= OnPickPhaseStarted;
            _serverListener.OnRoundResults -= HandleRoundResults;
            _serverListener.OnPlayerSelectedMap -= HandleOpponentSelectedMap;
            _matchBeatmapManager.CanNoLongerDiscardMaps -= HandleCanNoLongerDiscardMaps;
            _serverListener.OnMatchFinished -= HandleMatchFinished;
            _serverListener.OnCardsUpdated -= HandleCardsUpdated;
        }

        private void HideStandardLevelDetailControllerIfPresent()
        {
            if (!_standardLevelDetailViewManager.ManagedController.isActivated)
                return;
                
            _votingScreenNavigationController.PopViewController(() => {}, true);
            _soundEffectManager.CrossfadeToDefault();
        }

        protected override void BackButtonWasPressed(ViewController viewController)
        {
            _warningModalViewController.ParseOntoGameObject(viewController, "Are you sure you want to leave the match early?\nLeaving the match early could result in penalties!", async void () =>
            {
                try
                {
                    _soundEffectManager.CrossfadeToDefault();
                    _onMatchFinishedCallback?.Invoke();
                    await _serverListener.DisconnectAsync();
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            }, _warningModalViewController.Hide);
        }
    }
}