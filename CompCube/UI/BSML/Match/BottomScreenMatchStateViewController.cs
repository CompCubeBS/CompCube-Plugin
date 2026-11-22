using System.Collections;
using System.Globalization;
using System.Net.Http;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using CompCube.UI.BSML.Components.EnergyBar;
using HMUI;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateViewController : ViewController, IInitializable, IDisposable
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    [Inject] private readonly IServerListener _serverListener = null!;
    
    private BottomScreenMatchStateView _view = null!;

    public MatchTimerData CurrentTimer
    {
        set
        {
            StartCoroutine(WaitForSecondsAndInvoke());
            return;

            IEnumerator WaitForSecondsAndInvoke()
            {
                yield return new WaitUntil(() => _view != null);
                
                _view.TimerEndTime = value.EndTime;
                _view.StatusText = value.Title;
                
                yield return new WaitForSeconds(value.TimeSpan);
                
                value.OnTimerRanOut.Invoke();
            }
        }
    }

    private void Awake()
    {
        new BottomScreenMatchStateView().Bind(ref _view).Use(transform);
    }

    public void UpdateHealth(float redHealth, float blueHealth)
    {
        _view.RedHealth = Math.Max(0, redHealth);
        _view.BlueHealth = Math.Max(0, blueHealth);
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

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
    }

    private void HandleMatchCreated(MatchCreatedPacket packet)
    {
        _sharedCoroutineStarter.Run(HandleMatchCreatedCoroutine());
        return;
        
        IEnumerator HandleMatchCreatedCoroutine()
        {
            yield return new WaitUntil(() => _view != null);
            
            PopulateData(packet.Red, packet.Blue);
        
            SetStatus("Discard Phase");
            UpdateHealth(1f, 1f);
            UpdateMultiplier(1f);
        }
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
    }

    public class MatchTimerData(float timeSpan, string title, Action onTimerRanOut)
    {
        public readonly DateTime StartingTime = DateTime.Now;
        public readonly float TimeSpan = timeSpan;
        public DateTime EndTime => StartingTime.AddSeconds(TimeSpan);
        public readonly Action OnTimerRanOut = onTimerRanOut;
        public readonly string Title = title;
    }
}