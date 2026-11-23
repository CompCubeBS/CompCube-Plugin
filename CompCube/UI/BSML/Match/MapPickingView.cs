using CompCube_Models.Models.Map;
using CompCube.UI.BSML.Components.Card;
using CompCube.UI.BSML.Components.CustomLevelBar;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using SongCore;
using UnityEngine;
using static CompCube.UI.BSML.Match.MapPickingViewController;

namespace CompCube.UI.BSML.Match;

public class MapPickingView : ReactiveComponent
{
    public CCBeatmapData[] Beatmaps
    {
        get => _beatmapList.Value;
        set => _beatmapList.Value = value;
    }
    
    private State<CCBeatmapData[]> _beatmapList = null!; 

    private State<CCBeatmapData?> _activeBeatmap = null!;

    public CCBeatmapData? ActiveBeatmap
    {
        get => _activeBeatmap.Value;
        set => _activeBeatmap.Value = value;
    }
    
    public Action<CCBeatmapData>? OnBeatmapSelected { get; set; }

    public State CurrentState
    {
        get => _currentState.Value;
        set => _currentState.Value = value;
    }

    private State<State> _currentState = null!;
    
    public Action? OnPickButtonPressed { get; set; }
    
    public Action? OnDiscardButtonPressed { get; set; }
    
    public Action? OnHoldButtonPressed { get; set; }
    
    protected override GameObject Construct()
    {
        _activeBeatmap = Remember(null as CCBeatmapData);
        _beatmapList = Remember(new CCBeatmapData[]{});

        _currentState = Remember(State.Discard);

        var songDetailViewDimState = RememberAnimated(0f, 100.ms);

        return new Layout()
        {
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                
                FlexDirection = FlexDirection.Column,
                Gap = 5f
            },
            
            Children =
            {
                new Layout()
                {
                    FlexController =
                    {
                        AlignItems = Align.Center,
                        AlignContent = Align.Center,
                        JustifyContent = Justify.Center,
                        
                        FlexDirection = FlexDirection.Column,
                        
                        Gap = 2f
                    },
                    
                    Children =
                    {
                        new SongDetailView()
                        {
                            sBeatmap = _activeBeatmap.Map(x =>
                            {
                                songDetailViewDimState.TargetValue = x is null ? 0f : 1f;
                        
                                return x;
                            }),
                            WithinLayoutIfDisabled = true
                        },
                        
                        new BsPrimaryTextButton()
                        {
                            sInteractable = _currentState.Map(x => x is State.Pick),
                            sEnabled = _currentState.Map(x => x is State.Pick or State.OtherPlayerPick),
                            Text = "Play!",
                            FlexItem =
                            {
                                Size = new YogaVector(26, 7)
                            },
                            OnClick = () => OnPickButtonPressed?.Invoke()
                        },
                        new Layout()
                        {
                            FlexController =
                            {
                                FlexDirection = FlexDirection.Row,
                            },
                            
                            sEnabled = _currentState.Map(x => x is State.Discard),
                            
                            Children =
                            {
                                new BsTextButton()
                                { 
                                    Text = "Discard!",
                                    sEnabled = _currentState.Map(x => x is State.Discard),
                                    FlexItem =
                                    {
                                        Size = new YogaVector(26, 7)
                                    },
                                    OnClick = () => OnDiscardButtonPressed?.Invoke()
                                },
                                new BsTextButton()
                                { 
                                    Text = "Hold",
                                    sEnabled = _currentState.Map(x => x is State.Discard),
                                    FlexItem =
                                    {
                                        Size = new YogaVector(26, 7)
                                    },
                                    
                                    OnClick = () => OnHoldButtonPressed?.Invoke()
                                }
                            }
                        }
                    }
                }.With(x =>
                {
                    var g = x.Content.AddComponent<CanvasGroup>();
                    
                    songDetailViewDimState.AddCallback(a => g.alpha = a);
                }),
                
                new CardCollection()
                {
                    OnCardSelectionWasChanged = (beatmap) =>
                    {
                        _activeBeatmap.Value = beatmap;
                        OnBeatmapSelected?.Invoke(beatmap);
                    },
                    sData = _beatmapList.Map(x => x.ToList())
                }
            }
        }.Use();
    }
}