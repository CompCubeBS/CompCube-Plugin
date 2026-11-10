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
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateViewController : ViewController
{
    private BottomScreenMatchStateView _view = null!;
    
    private void Awake()
    {
        new BottomScreenMatchStateView().Bind(ref _view).Use(transform);
    }

    public void UpdateHealth(float redHealth, float blueHealth)
    {
        _view.RedHealth = redHealth;
        _view.BlueHealth = blueHealth;
    }

    public void UpdateMultiplier(float multiplier)
    {
        _view.Multiplier = multiplier;
    }

    public void SetStatus(string status)
    {
        _view.StatusText = status;
    }
    
    public void UpdateRound(int round) => SetStatus($"Round {round}");

    public void PopulateData(UserProfile red, UserProfile blue)
    {
        _view.RedProfile = red;
        _view.BlueProfile = blue;
    }
}