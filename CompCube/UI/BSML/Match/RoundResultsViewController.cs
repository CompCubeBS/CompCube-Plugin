using System.Collections;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.EarlyLeaveWarning;
using HMUI;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    public class RoundResultsViewController : ViewController
    {
        private const float DamageTextAnimationTimeSeconds = 3f;
        
        [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        [Inject] private readonly MatchStateManager _matchStateManager = null!;

        [Inject] private readonly SiraLog _siraLog = null!;
        
        private RoundResultsView _roundResultsView = null!;

        void Awake()
        {
            new RoundResultsView(DamageTextAnimationTimeSeconds).Bind(ref _roundResultsView).Use(transform);
        }
        
        public void PopulateData(MatchScore redScore, MatchScore blueScore, float damageAmount, float multiplier, CCBeatmapData votingMap)
        {
            _sharedCoroutineStarter.Run(PopulateDataCoroutine());
            return;

            IEnumerator PopulateDataCoroutine()
            {
                yield return new WaitUntil(() => isActivated);
            
                var redWon = redScore.Score.Points >= blueScore.Score.Points;
                
                var winnerScore = redWon ? redScore : blueScore;
                var loserScore = redWon ? blueScore : redScore;
            
                var winner = redWon ? _matchStateManager.RedPlayer : _matchStateManager.BluePlayer;
                var loser = redWon ? _matchStateManager.BluePlayer : _matchStateManager.RedPlayer;
                
                var selfWon = (redWon && _matchStateManager.IsRedTeam) || (!redWon && !_matchStateManager.IsRedTeam);

                _roundResultsView.DidWin = selfWon;
                _roundResultsView.WinningScore = winnerScore;
                _roundResultsView.LosingScore = loserScore;
                _roundResultsView.MapPlayed = votingMap;
                
                _roundResultsView.ReplayButtonActive = winner.PlatformId != _matchStateManager.Self.PlatformId;
                _roundResultsView.ReplayButtonInteractable = _matchStateManager.RemainingReplays > 0;

                _roundResultsView.ReplayButtonClickedCallback = HandleReplayButtonClicked;
                
                _roundResultsView.ResetDamageAmountImmediate();
                
                yield return new WaitForSeconds(1f);
                
                _roundResultsView.Damage = damageAmount * multiplier;
                
                yield return new WaitForSeconds(DamageTextAnimationTimeSeconds + 2f);

                if (!isActivated)
                    yield return null;
                
                _bottomScreenMatchStateViewController.UpdateHealth(redWon ? _matchStateManager.RedHealth : _matchStateManager.RedHealth - damageAmount * multiplier, !redWon ? _matchStateManager.BlueHealth : _matchStateManager.BlueHealth - damageAmount * multiplier);

                /*_customLevelbar?.Setup(votingMap);




                ReplayButtonActive = !selfWon;

                ReplayButtonInteractable = !selfWon && _matchStateManager.RemainingReplays > 0;

                WinnerScoreText = FormatScore(winnerScore, winner, 1);
                LoserScoreText = FormatScore(loserScore, loser, 2);

                DamageText = (damageAmount * _matchStateManager.DamageMultiplier).ToString("P", CultureInfo.InvariantCulture);*/
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
                        _roundResultsView.ReplayButtonInteractable = false;

                        await _matchStateManager.UseReplay();
                    }
                    catch (Exception e)
                    {
                        _siraLog.Error(e);
                    }
                });
        }
    }
}
