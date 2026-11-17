using System.Globalization;
using CompCube_Models.Models.Map;
using CompCube.Configuration;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;

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
        
        return new Layout()
        {
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Column,
                
                Gap = 7.5f
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
                new BsLevelBar()
                {
                    sData = _beatmap.Map(x => x),
                    
                    FlexItem =
                    {
                        Position = new YogaFrame()
                        {
                            left = -80,
                            top = -17.5f
                        }
                    },
                },
                
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Column,
                        
                        Gap = 2.5f
                    },
                    
                    Children =
                    {
                        CreateBgText(_beatmap, false),
                        CreateBgText(_beatmap, true),
                    }
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
                        }.InNamedRail("Score Submission")
                    }
                },
                
                new Label()
                {
                    FontStyle = FontStyles.Italic,
                    sText = _secondsUntilTimerEnds.Map(x => $"Starting in {(((int) _secondsUntilTimerEnds.Value) + 1).ToString(CultureInfo.InvariantCulture)}...")
                    
                }
            }
        }.Use();
    }

    protected override void OnUpdate()
    {
        _secondsUntilTimerEnds?.Value = (float) (TimerEndTime - DateTime.Now).TotalSeconds;
    }

    private string GetBeatmapDifficultyText(CCBeatmapData.DifficultyType difficulty)
    {
        if (difficulty == CCBeatmapData.DifficultyType.ExpertPlus)
            return "Expert+";

        return difficulty.ToString();
    }

    [StateGen]
    private Background CreateBgText(State<CCBeatmapData> beatmap, bool forCategory)
    {
        return new Background()
        {
            FlexItem =
            {
                Size = new YogaVector()
                {
                    x = 85,
                    y = 5
                }
            },
            
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent =  Justify.Center,
            },
            
            Children =
            {
                new Label()
                {
                    Alignment = TextAlignmentOptions.Midline,
                    sText = beatmap.Map(x =>
                    {
                        if (x is null)
                            return "example";
                        
                        return forCategory
                            ? $"Category: {x.MapCategory.ToString()}"
                            : GetBeatmapDifficultyText(x.Difficulty);
                    }),
                    FontStyle = FontStyles.Italic,
                }
            }
        }.AsBlurBackground();
    }
}