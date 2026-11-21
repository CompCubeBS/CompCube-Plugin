using System.Collections;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.Map;
using CompCube.Configuration;
using CompCube.UI.BSML.Components.CustomLevelBar;
using HMUI;
using CompCube.Extensions;
using Reactive;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CompCube.UI.BSML.Match;

public class WaitingForMatchToStartViewController() : ViewController
{
    [Inject] private readonly PluginConfig _config = null!;
    
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
        StartCoroutine(PopulateData());
        return;

        IEnumerator PopulateData()
        {
            yield return new WaitUntil(() => isActivated);
            
            _view.Beatmap = beatmapData;
            _view.TimerEndTime = timerEndTime;
        }
    }
}