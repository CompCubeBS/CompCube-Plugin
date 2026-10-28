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
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    [ViewDefinition("CompCube.UI.BSML.Match.RoundResultsView.bsml")]
    public class RoundResultsViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly MatchStateManager _matchStateManager = null!;
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        [Inject] private readonly SiraLog _siraLog = null!;
        
        [UIValue("titleBgColor")] private string TitleBgColor { get; set; } = "#FFA500";
        [UIValue("titleText")] private string TitleText { get; set; } = "";

        [UIValue("winnerScoreText")] private string WinnerScoreText { get; set; } = "";
        [UIValue("loserScoreText")] private string LoserScoreText { get; set; } = "";
        
        [UIValue("damageText")] private string DamageText { get; set; } = "";
        
        [UIValue("replayButtonActive")] private bool ReplayButtonActive { get; set; } = false;
        
        [UIValue("replayButtonInteractable")] private bool ReplayButtonInteractable { get; set; } = false;

        private CustomLevelBar? _customLevelbar;

        [UIAction("#post-parse")]
        void PostParse()
        {
            _customLevelbar ??= Resources.FindObjectsOfTypeAll<CustomLevelBar>()
                .First(i => i.name == "RoundResultsLevelBar");
        }

        [UIAction("replayButtonOnClick")]
        async void ReplayButtonOnClick()
        {
            try
            {
                ReplayButtonInteractable = false;
                
                NotifyPropertyChanged(nameof(ReplayButtonInteractable));

                await _matchStateManager.UseReplay();
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }
        
        public void PopulateData(Score redScore, Score blueScore, float damageAmount, float multiplier, VotingMap votingMap)
        {
            _sharedCoroutineStarter.Run(PopulateDataCoroutine());
            return;

            IEnumerator PopulateDataCoroutine()
            {
                yield return new WaitUntil(() => isActivated);
                
                TitleText = "Results";
            
                _customLevelbar?.Setup(votingMap);
            
                var redWon = redScore.Points >= blueScore.Points;
            
                var winnerScore = redWon ? redScore : blueScore;
                var loserScore = redWon ? blueScore : redScore;
            
                var winner = redWon ? _matchStateManager.RedPlayer : _matchStateManager.BluePlayer;
                var loser = redWon ? _matchStateManager.BluePlayer : _matchStateManager.RedPlayer;
                
                var selfWon = (redWon && _matchStateManager.IsRedTeam) || (!redWon && !_matchStateManager.IsRedTeam);

                ReplayButtonActive = !selfWon;

                ReplayButtonInteractable = !selfWon && _matchStateManager.RemainingReplays > 0;
            
                WinnerScoreText = FormatScore(winnerScore, winner, 1);
                LoserScoreText = FormatScore(loserScore, loser, 2);
            
                DamageText = (damageAmount * _matchStateManager.DamageMultiplier).ToString("P", CultureInfo.InvariantCulture);
            
                NotifyPropertyChanged(null);
            }
        }

        private string FormatScore(Score score, UserStatistics user, int placement) =>
            $"{(placement)}. {user.GetFormattedUserName()} - " +
            $"{(score.RelativeScore * 100):F}% " +
            $"{(score.FullCombo ? "FC".FormatWithHtmlColor("#90EE90") : $"{score.Misses}x".FormatWithHtmlColor("#FF7F7F"))}" +
            $"{(score.ProMode ? " (PM)" : "")}";
    }
}
