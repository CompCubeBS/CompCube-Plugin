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
                
                FlexDirection = FlexDirection.Column,
                Gap = 10f
            },
            
            Children =
            {
                new SongDetailView()
                {
                    sEnabled = _beatmap.Map(x => x is not null),
                    sBeatmap = _beatmap.Map(x => x),
                    WithinLayoutIfDisabled = true
                },
                
                new CardCollection()
                {
                    OnCardWasSelected = (beatmap) => _beatmap.Value = beatmap,
                    Data = Loader.CustomLevels.Where(i => i.Value.GetBeatmapKeys().Any(j => j.beatmapCharacteristic.serializedName == "Standard" && j.difficulty == BeatmapDifficulty.ExpertPlus)).Take(5).Select(i => new CCBeatmapData("0", i.Value.levelID.Substring("custom_level_".Length), CCBeatmapData.DifficultyType.ExpertPlus, CCBeatmapData.Category.Accuracy, 0, 0)).ToList()
                }
            }
        }.Use();
    }
}