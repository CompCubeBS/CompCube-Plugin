using System.Collections;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Game.MatchState;
using CompCube.Interfaces;
using CompCube.UI.BSML.EarlyLeaveWarning;
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
    
    [Inject] private readonly SiraLog _siraLog = null!;
    
    private MapPickingView _mapPickingView = null!;

    public event Action<CCBeatmapData>? OnBeatmapCardSelected;
    
    private void Awake()
    {
        new MapPickingView().Bind(ref _mapPickingView).Use(transform);

        _mapPickingView.OnBeatmapSelected = (beatmap) => OnBeatmapCardSelected?.Invoke(beatmap);
    }

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
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
    }
}