﻿using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube.Models;
using JetBrains.Annotations;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.Components.CustomLevelBar;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    [ViewDefinition("CompCube.UI.BSML.Match.RoundResultsView.bsml")]
    public class RoundResultsViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly MatchStateManager _matchStateManager = null!;
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        
        [UIValue("titleBgColor")] private string TitleBgColor { get; set; } = "#FFA500";
        [UIValue("titleText")] private string TitleText { get; set; } = "";

        [UIValue("winnerScoreText")] private string WinnerScoreText { get; set; } = "";
        [UIValue("loserScoreText")] private string LoserScoreText { get; set; } = "";
        
        [UIValue("damageText")] private string DamageText { get; set; } = "";

        private CustomLevelBar? _customLevelbar;

        [UIAction("#post-parse")]
        void PostParse()
        {
            _customLevelbar ??= Resources.FindObjectsOfTypeAll<CustomLevelBar>()
                .First(i => i.name == "RoundResultsLevelBar");
        }
        
        public void PopulateData(RoundResultsMessage results, float multiplier, VotingMap votingMap)
        {
            _sharedCoroutineStarter.Run(PopulateDataCoroutine());
            return;

            IEnumerator PopulateDataCoroutine()
            {
                yield return new WaitUntil(() => isActivated);
                
                TitleText = "Results";
            
                _customLevelbar?.Setup(votingMap);
            
                var redWon = results.RedScore.Points >= results.BlueScore.Points;
            
                var winnerScore = redWon ? results.RedScore : results.BlueScore;
                var loserScore = redWon ? results.BlueScore : results.RedScore;
            
                var winner = redWon ? _matchStateManager.RedPlayer : _matchStateManager.BluePlayer;
                var loser = redWon ? _matchStateManager.BluePlayer : _matchStateManager.RedPlayer;
            
                WinnerScoreText = FormatScore(winnerScore, winner, 1);
                LoserScoreText = FormatScore(loserScore, loser, 2);
            
                DamageText = ((winnerScore.Accuracy - loserScore.Accuracy) * _matchStateManager.DamageMultiplier).ToString("P", CultureInfo.InvariantCulture);
            
                NotifyPropertyChanged(null);
            }
        }

        private string FormatScore(Score score,CompCube.Models.UserInfo user, int placement) =>
            $"{(placement)}. {user.GetFormattedUserName()} - " +
            $"{(score.Accuracy * 100):F}% " +
            $"{(score.FullCombo ? "FC".FormatWithHtmlColor("#90EE90") : $"{score.Misses}x".FormatWithHtmlColor("#FF7F7F"))}" +
            $"{(score.ProMode ? " (PM)" : "")}";
    }
}
