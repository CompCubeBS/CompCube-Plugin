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
    private State<string> _pickButtonText = null!;

    private State<CCBeatmapData?> _beatmap = null!;
    
    protected override GameObject Construct()
    {
        _beatmap = Remember(null as CCBeatmapData);
        _pickButtonText = Remember("");

        return new Layout()
        {
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
            },
            
            Children =
            {
                new SongDetailView()
                {
                    sEnabled = _beatmap.Map(x => x is not null),
                    sBeatmap = _beatmap.Map(x => x)
                },
                
                new CardCollection()
                {
                    OnCardWasSelected = (beatmap) => _beatmap.Value = beatmap,
                    Data = Loader.CustomLevels.Take(5).Select(i => new CCBeatmapData("0", i.Key, CCBeatmapData.DifficultyType.ExpertPlus, CCBeatmapData.Category.Accuracy, 0, 0)).ToList()
                }
            }
        }.Use();
    }
}