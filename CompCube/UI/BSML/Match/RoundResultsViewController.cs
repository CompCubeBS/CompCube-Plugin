using System.Collections;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using CompCube.UI.BSML.EarlyLeaveWarning;
using HMUI;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    public class RoundResultsViewController : ViewController, IInitializable, IDisposable
    {
        private const float DamageTextAnimationTimeSeconds = 3f;

        [Inject] private readonly IServerListener _serverListener = null!;
        
        [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        [Inject] private readonly MatchStateManager _matchStateManager = null!;

        [Inject] private readonly SiraLog _siraLog = null!;
        
        private RoundResultsView _view = null!;
        
        private CCBeatmapData? _beatmapData = null;

        void Awake()
        {
            new RoundResultsView(DamageTextAnimationTimeSeconds).Bind(ref _view).Use(transform);
        }
        
        public void PopulateData(MatchScore redScore, MatchScore blueScore, float damageAmount, float multiplier, CCBeatmapData votingMap)
        {
            _sharedCoroutineStarter.Run(PopulateDataCoroutine());
            return;

            IEnumerator PopulateDataCoroutine()
            {
                yield return new WaitUntil(() => _view != null);
            
                var redWon = redScore.Score.Points >= blueScore.Score.Points;
                
                var winnerScore = redWon ? redScore : blueScore;
                var loserScore = redWon ? blueScore : redScore;
            
                var winner = redWon ? _matchStateManager.RedPlayer : _matchStateManager.BluePlayer;
                var loser = redWon ? _matchStateManager.BluePlayer : _matchStateManager.RedPlayer;
                
                var selfWon = (redWon && _matchStateManager.IsRedTeam) || (!redWon && !_matchStateManager.IsRedTeam);

                _view.DidWin = selfWon;
                _view.WinningScore = winnerScore;
                _view.LosingScore = loserScore;
                _view.MapPlayed = votingMap;
                
                _view.ReplayButtonActive = winner.PlatformId != _matchStateManager.Self.PlatformId;
                _view.ReplayButtonInteractable = _matchStateManager.RemainingReplays > 0;

                _view.ReplayButtonClickedCallback = HandleReplayButtonClicked;
                
                _view.ResetDamageAmountImmediate();
                
                yield return new WaitForSeconds(1f);
                
                _view.Damage = damageAmount * multiplier;
                
                yield return new WaitForSeconds(DamageTextAnimationTimeSeconds + 2f);

                if (!isActivated)
                    yield return null;
                
                _bottomScreenMatchStateViewController.UpdateHealth(redWon ? _matchStateManager.RedHealth : _matchStateManager.RedHealth - damageAmount * multiplier, !redWon ? _matchStateManager.BlueHealth : _matchStateManager.BlueHealth - damageAmount * multiplier);
            }
        }

        private void HandleReplayButtonClicked()
        {
            YesOrNoDialogModal.CreateOnTransform(transform, "NOTE:\nYou can only call one replay per match, and the\nhighest scores from either player will be used.\n\nWould you like to call a replay?",
                true, 
                async void () =>
                {
                    try
                    {
                        _view.ReplayButtonInteractable = false;

                        await _matchStateManager.UseReplay();
                    }
                    catch (Exception e)
                    {
                        _siraLog.Error(e);
                    }
                });
        }

        public void Initialize()
        {
            _serverListener.OnRoundResults += HandleRoundResults;
            _serverListener.OnPlayerSelectedMap += HandlePlayerSelectedMap;
        }

        private void HandlePlayerSelectedMap(PlayerSelectedMapPacket packet) => _beatmapData = packet.Map;

        private void HandleRoundResults(RoundResultsPacket packet)
        {
            PopulateData(new MatchScore(_matchStateManager.RedPlayer, packet.RedScore), new MatchScore(_matchStateManager.BluePlayer, packet.BlueScore), packet.DamageAmount, _matchStateManager.DamageMultiplier, _beatmapData!);
        }

        public void Dispose()
        {
            _serverListener.OnRoundResults -= HandleRoundResults;
            _serverListener.OnPlayerSelectedMap -= HandlePlayerSelectedMap;
        }
    }
}
