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
using Timer = System.Timers.Timer;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateViewController : ViewController, IInitializable, IDisposable
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    [Inject] private readonly IServerListener _serverListener = null!;

    [Inject] private readonly MatchBeatmapManager _matchBeatmapManager = null!;
    [Inject] private readonly MatchStateManager _matchStateManager = null!;
    
    private BottomScreenMatchStateView _view = null!;

    public MatchTimerData? Timer
    {
        get => _view.CurrentTimer;
        set
        {
            _view.CurrentTimer?.Cancel();
            _view.CurrentTimer = value;
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

    public void PopulateData(UserProfile red, UserProfile blue)
    {
        _view.RedProfile = red;
        _view.BlueProfile = blue;
    }

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnRoundResults += HandleRoundResults;
    }

    private void HandleRoundResults(RoundResultsPacket results)
    {
        _view.CurrentTimer = new MatchTimerData(TimeSpan.FromSeconds(10), "Results", null);
    }

    private void HandleMatchCreated(MatchCreatedPacket packet)
    {
        _sharedCoroutineStarter.Run(HandleMatchCreatedCoroutine());
        return;
        
        IEnumerator HandleMatchCreatedCoroutine()
        {
            yield return new WaitUntil(() => _view != null);
            
            PopulateData(packet.Red, packet.Blue);
        
            UpdateHealth(1f, 1f);
            UpdateMultiplier(1f);
        }
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
    }

    public class MatchTimerData
    {
        public readonly Timer Timer;
        
        public readonly string Title;

        public readonly DateTime StartTime = DateTime.Now;
        
        public readonly TimeSpan Duration;

        public TimeSpan TimeRemaining => (StartTime + Duration) - DateTime.Now;
        
        private bool _isCancelled = false;

        public MatchTimerData(TimeSpan timeSpan, string title, Action? onTimerRanOut)
        {
            Title = title;
            Duration = timeSpan;
            
            Timer = new Timer()
            {
                AutoReset = false,
                Enabled = true,
                Interval = timeSpan.TotalMilliseconds
            };
            
            Timer.Elapsed += (_, _) =>
            {
                if (_isCancelled)
                    return;
                onTimerRanOut?.Invoke();
            };
        }
        
        public void Cancel()
        {
            _isCancelled = true;
        }
    }
}