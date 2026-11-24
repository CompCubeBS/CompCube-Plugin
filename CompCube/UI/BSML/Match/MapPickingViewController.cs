using System.Collections;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Extensions;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using CompCube.UI.BSML.EarlyLeaveWarning;
using CompCube.UI.BSML.Modal;
using CompCube.UI.Sound;
using HMUI;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class MapPickingViewController : ViewController, IInitializable, IDisposable
{
    [Inject] private readonly SharedCoroutineStarter _coroutineStarter = null!;
    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly BottomScreenMatchStateViewController _bottomScreenMatchStateViewController = null!;
    [Inject] private readonly MatchStateManager _matchStateManager = null!;
    [Inject] private readonly MatchBeatmapManager _matchBeatmapManager = null!;

    [Inject] private readonly SoundEffectManager _soundEffectManager = null!;
    
    [Inject] private readonly SiraLog _siraLog = null!;
    
    private MapPickingView _mapPickingView = null!;

    private bool _isShowingFinalCards = false;

    public event Action<CCBeatmapData>? OnBeatmapCardSelected;
    
    private void Awake()
    {
        new MapPickingView().Bind(ref _mapPickingView).Use(transform);

        _mapPickingView.OnBeatmapSelected = (beatmap) =>
        {
            if (beatmap is null)
            {
                _soundEffectManager.CrossfadeToDefault();
                return;
            }
            
            _soundEffectManager.PlayBeatmapLevelPreview(beatmap.GetBeatmapLevel()!);
            _mapPickingView.ActiveBeatmap = beatmap;
            _mapPickingView.Beatmaps = _matchBeatmapManager.AvailablePicks.ToArray();
            OnBeatmapCardSelected?.Invoke(beatmap);
        };
        
        _mapPickingView.OnDiscardButtonPressed = () =>
        {
            if (_mapPickingView.ActiveBeatmap is null)
                return;
            
            _matchBeatmapManager.DiscardMap(_mapPickingView.ActiveBeatmap);
            _mapPickingView.ActiveBeatmap = null;
            _mapPickingView.Beatmaps = _matchBeatmapManager.AvailablePicks.ToArray();
        };

        _mapPickingView.OnHoldButtonPressed = () =>
        {
            _matchBeatmapManager.SkipDiscardingMaps();
            _mapPickingView.CurrentState = State.PostDiscard;
            _mapPickingView.Beatmaps = _matchBeatmapManager.AvailablePicks.ToArray();

            _bottomScreenMatchStateViewController.Timer = null;
        };

        _mapPickingView.OnPickButtonPressed = () =>
        {
            _bottomScreenMatchStateViewController.Timer = null;
            _matchBeatmapManager.SelectBeatmap(_mapPickingView.ActiveBeatmap!);
        };
    }

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
        _serverListener.OnCardsUpdated += HandleCardsUpdated;
    }

    private void HandleCardsUpdated(UpdateCardsPacket packet)
    {
        _mapPickingView.ActiveBeatmap = null;
        
        _isShowingFinalCards = true;
        _bottomScreenMatchStateViewController.Timer = null;
        
        DownloadingBeatmapsModal.CreateOnTransformAndDownloadBeatmaps(transform,
            packet.Maps.Select(i => i.Hash).ToArray(),
            () =>
            {
                _mapPickingView.Beatmaps = packet.Maps;
                
                _bottomScreenMatchStateViewController.Timer = new(TimeSpan.FromSeconds(5), "Discard Phase", () =>
                {
                    _isShowingFinalCards = false;
                });
            });
    }

    private void HandlePickPhaseStarted(StartPickPhasePacket packet)
    {
        _mapPickingView.ActiveBeatmap = null;
        
        _coroutineStarter.Run(Yeah());
        return;

        IEnumerator Yeah()
        {
            yield return new WaitForSeconds(1f);
            
            yield return new WaitUntil(() => !_isShowingFinalCards);
            
            PhasePopupModal.CreateOnTransform(transform, "Pick Phase", $"{(packet.IsOwnPick ? _matchStateManager.Self : _matchStateManager.Opponent).GetFormattedUserName()}'s Pick");
        
            _mapPickingView.CurrentState = packet.IsOwnPick ? State.Pick : State.OtherPlayerPick;
        
            _mapPickingView.Beatmaps = packet.AvailableMaps;
            _mapPickingView.ActiveBeatmap = null;
            
            var user = _matchStateManager.Self;

            if (!packet.IsOwnPick)
                user = _matchStateManager.Opponent;
        
            _bottomScreenMatchStateViewController.Timer = new BottomScreenMatchStateViewController.MatchTimerData(TimeSpan.FromSeconds(30), $"Pick Phase\n<size=50%>{user.GetFormattedUserName()}'s Pick</size>", () =>
            {
                _matchBeatmapManager.SelectBeatmap(_matchBeatmapManager.AvailablePicks.First());
            });
        }
    }

    private void HandleMatchCreated(MatchCreatedPacket packet)
    {
        _coroutineStarter.Run(StartMapDownloadAndPopulateData());
        return;

        IEnumerator StartMapDownloadAndPopulateData()
        {
            yield return new WaitUntil(() => _mapPickingView != null);
                
            DownloadingBeatmapsModal.CreateOnTransformAndDownloadBeatmaps(transform,
                packet.InitialMaps.Select(i => i.Hash).ToArray(),
                () =>
                {
                    _bottomScreenMatchStateViewController.Timer = new(TimeSpan.FromSeconds(30), "Discard Phase",
                        () =>
                        {
                            _matchBeatmapManager.SkipDiscardingMaps();
                        });
                    _mapPickingView.Beatmaps = packet.InitialMaps;
                });
        }
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
    }
    
    public enum State
    {
        Discard,
        PostDiscard,
        Pick,
        OtherPlayerPick
    }
}