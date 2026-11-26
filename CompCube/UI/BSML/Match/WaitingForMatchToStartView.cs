using System.Globalization;
using System.Reflection;
using CompCube_Models.Models.Map;
using CompCube.Configuration;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using SiraUtil.Extras;
using TMPro;
using UnityEngine;
using Zenject;
using Image = Reactive.Components.Basic.Image;

namespace CompCube.UI.BSML.Match;

public class WaitingForMatchToStartView : ReactiveComponent
{
    public CCBeatmapData Beatmap
    {
        get => _beatmap.Value;
        set => _beatmap.Value = value;
    }
    
    private State<CCBeatmapData> _beatmap = null!;

    public DateTime TimerEndTime { get; set; } = DateTime.Now;

    private State<float>? _secondsUntilTimerEnds = null;

    public event Action<bool>? OnScoreSubmissionToggled;
    
    protected override GameObject Construct()
    {
        _beatmap = Remember(null as CCBeatmapData);
        _secondsUntilTimerEnds = Remember(0f);

        var rail = new NamedRail();
        
        var layout = new Layout()
        {
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Column,
                
                Gap = 2.5f
            },
            
            FlexItem =
            {
                Size = new YogaVector()
                {
                    y = 100
                }
            },
            
            Children =
            {
                
                new SongDetailView()
                {
                    sBeatmap = _beatmap.Map(x => x)
                },
                
                
                
                new Layout()
                {
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 85,
                            y = 7
                        }
                    },
                    
                    Children =
                    {
                        new BsToggle()
                        {
                            Animated = true,
                    
                            Toggled = true,
                    
                            OnToggleStateChanged = (value) => OnScoreSubmissionToggled?.Invoke(value),
                        }.InNamedRail("Score Submission").Bind(ref rail)
                    }
                },
                
                new Label()
                {
                    FontStyle = FontStyles.Italic,
                    sText = _secondsUntilTimerEnds.Map(x => $"Starting in {(((int) _secondsUntilTimerEnds.Value) + 1).ToString(CultureInfo.InvariantCulture)}...")
                }
            }
        }.Use();

        rail.Label.Alignment = TextAlignmentOptions.Midline;
        rail.Label.FontStyle = FontStyles.Italic;

        return layout;
    }

    protected override void OnUpdate()
    {
        _secondsUntilTimerEnds?.Value = (float) (TimerEndTime - DateTime.Now).TotalSeconds;
    }
}