using CompCube_Models.Models.Map;
using CompCube.UI.BSML.Components.Card;
using CompCube.UI.BSML.Components.CustomLevelBar;
using Reactive;
using Reactive.Yoga;
using SongCore;
using UnityEngine;

namespace CompCube.UI.BSML.Match;

public class MapPickingView : ReactiveComponent
{
    public CCBeatmapData[] Beatmaps
    {
        get => _beatmapList.Value;
        set => _beatmapList.Value = value;
    }
    
    private State<CCBeatmapData[]> _beatmapList = null!; 
    
    private State<string> _pickButtonText = null!;

    private State<CCBeatmapData?> _activeBeatmap = null!;
    
    public Action<CCBeatmapData>? OnBeatmapSelected { get; set; }
    
    protected override GameObject Construct()
    {
        _activeBeatmap = Remember(null as CCBeatmapData);
        _pickButtonText = Remember("");
        _beatmapList = Remember(new CCBeatmapData[]{});

        var songDetailViewDimState = RememberAnimated(0f, 100.ms);

        return new Layout()
        {
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                
                FlexDirection = FlexDirection.Column,
                Gap = 10f
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