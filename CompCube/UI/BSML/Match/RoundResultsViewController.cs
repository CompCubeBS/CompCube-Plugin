﻿using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.ServerPackets;
using JetBrains.Annotations;
using CompCube.Extensions;
using CompCube.Game;
using CompCube.Game.MatchState;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    [ViewDefinition("CompCube.UI.BSML.Match.RoundResultsView.bsml")]
    public class RoundResultsViewController : BSMLAutomaticViewController
    {
        [Inject] private readonly MatchStateManager _matchStateManager = null!;
        
        [UIValue("titleBgColor")] private string TitleBgColor { get; set; } = "#FFA500";
        [UIValue("titleText")] private string TitleText { get; set; } = "";

        [UIValue("winnerScoreText")] private string WinnerScoreText { get; set; } = "";
        [UIValue("loserScoreText")] private string LoserScoreText { get; set; } = "";
        
        [UIValue("damageText")] private string DamageText { get; set; } = "";
        
        public void PopulateData(RoundResultsPacket results, float multiplier)
        {
            TitleText = "Results";
            
            var redWon = results.RedScore.Points >= results.BlueScore.Points;
            
            var winnerScore = redWon ? results.RedScore : results.BlueScore;
            var loserScore = redWon ? results.BlueScore : results.RedScore;
            
            var winner = redWon ? _matchStateManager.RedPlayer : _matchStateManager.BluePlayer;
            var loser = redWon ? _matchStateManager.BluePlayer : _matchStateManager.RedPlayer;
            
            WinnerScoreText = FormatScore(winnerScore, winner, 1);
            LoserScoreText = FormatScore(loserScore, loser, 2);
            
            DamageText = ((winnerScore.RelativeScore - loserScore.RelativeScore) * _matchStateManager.DamageMultiplier).ToString("P", CultureInfo.InvariantCulture);
            
            NotifyPropertyChanged(null);
        }

        private string FormatScore(Score score,CompCube_Models.Models.ClientData.UserInfo user, int placement) => 
            $"{(placement)}. {user.GetFormattedUserName()} - " +
            $"{(score.RelativeScore * 100):F}% " +
            $"{(score.FullCombo ? "FC".FormatWithHtmlColor("#90EE90") : $"{score.Misses}x".FormatWithHtmlColor("#FF7F7F"))}" +
            $"{(score.ProMode ? " (PM)" : "")}";
    }
}