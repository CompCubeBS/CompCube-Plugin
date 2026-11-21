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
    
    protected override GameObject Construct()
    {
        _activeBeatmap = Remember(null as CCBeatmapData);
        _pickButtonText = Remember("");
        _beatmapList = Remember(new CCBeatmapData[]{});

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
                new SongDetailView()
                {
                    sEnabled = _activeBeatmap.Map(x => x is not null),
                    sBeatmap = _activeBeatmap.Map(x => x),
                    WithinLayoutIfDisabled = true
                },
                
                new CardCollection()
                {
                    OnCardSelectionWasChanged = (beatmap) => _activeBeatmap.Value = beatmap,
                    sData = _beatmapList.Map(x => x.ToList())
                }
            }
        }.Use();
    }
}