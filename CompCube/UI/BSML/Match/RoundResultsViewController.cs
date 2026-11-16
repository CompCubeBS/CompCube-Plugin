using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using JetBrains.Annotations;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.Components.CustomLevelBar;
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
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        [Inject] private readonly MatchStateManager _matchStateManager = null!;
        
        private RoundResultsView _roundResultsView = null!;

        void Awake()
        {
            new RoundResultsView().Bind(ref _roundResultsView).Use(transform);
        }
        
        public void PopulateData(MatchScore redScore, MatchScore blueScore, float damageAmount, float multiplier, VotingMap votingMap)
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

                /*_customLevelbar?.Setup(votingMap);




                ReplayButtonActive = !selfWon;

                ReplayButtonInteractable = !selfWon && _matchStateManager.RemainingReplays > 0;

                WinnerScoreText = FormatScore(winnerScore, winner, 1);
                LoserScoreText = FormatScore(loserScore, loser, 2);

                DamageText = (damageAmount * _matchStateManager.DamageMultiplier).ToString("P", CultureInfo.InvariantCulture);*/
            }
        }
    }
}
