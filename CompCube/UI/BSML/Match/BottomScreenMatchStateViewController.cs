using System.Collections;
using System.Globalization;
using System.Net.Http;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.UI.BSML.Components.EnergyBar;
using HMUI;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateViewController : ViewController
{
    [Inject] private readonly MatchStateManager _matchStateManager = null!;
    
    private void Awake()
    {
        new BottomScreenMatchStateView(_matchStateManager).Use(transform);
    }

    public void UpdateHealth(float redHealth, float blueHealth)
    {
        
    }

    public void UpdateMultiplier(float multiplier)
    {
        
    }

    public void SetStatus(string status)
    {
        
    }
    
    public void UpdateRound(int round) => SetStatus($"Round {round}");

    public void PopulateData(UserProfile red, UserProfile blue)
    {
        
    }
}