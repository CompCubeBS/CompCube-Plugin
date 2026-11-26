using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Configuration;
using CompCube.UI.BSML.Components.CustomLevelBar;
using HMUI;
using CompCube.Extensions;
using CompCube.Interfaces;
using CompCube.UI.BSML.EarlyLeaveWarning;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class WaitingForMatchToStartViewController() : ViewController, IInitializable, IDisposable
{
    [Inject] private readonly PluginConfig _config = null!;
    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    private WaitingForMatchToStartView _view = null!;
    
    private void Awake()
    {
        new WaitingForMatchToStartView().Bind(ref _view).Use(transform);
        
        _view.OnScoreSubmissionToggled += HandleScoreSubmissionToggled;
    }

    private void HandleScoreSubmissionToggled(bool toggled)
    {
        _config.ScoreSubmission = toggled;
    }

    public void PopulateDataCoroutine(CCBeatmapData beatmapData, DateTime timerEndTime)
    {
        _sharedCoroutineStarter.Run(PopulateData());
        return;

        IEnumerator PopulateData()
        {
            yield return new WaitUntil(() => isActivated);

            DownloadingBeatmapsModal.CreateOnTransformAndDownloadBeatmaps(transform, [beatmapData.Hash], () =>
            {
                _view.Beatmap = beatmapData;
                _view.TimerEndTime = timerEndTime;
            });
        }
    }

    public void Initialize()
    {
        _serverListener.OnPlayerSelectedMap += HandlePlayerSelectedMap;
    }

    private void HandlePlayerSelectedMap(PlayerSelectedMapPacket packet)
    {
        PopulateDataCoroutine(packet.Map, DateTime.Now.AddSeconds(15));
    }

    public void Dispose()
    {
        _serverListener.OnPlayerSelectedMap += HandlePlayerSelectedMap;
    }
}