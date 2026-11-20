using CompCube_Models.Models.Map;
using CompCube.UI.BSML.Components.Card;
using Reactive;
using SongCore;
using UnityEngine;

namespace CompCube.UI.BSML.Match;

public class MapPickingView : ReactiveComponent
{
    private State<string> _pickButtonText = null!;
    
    protected override GameObject Construct()
    {
        _pickButtonText = Remember("");

        return new Layout()
        {
            Children =
            {
                new CardCollection()
                {
                    Data = Loader.CustomLevels.Take(5).Select(i => new CCBeatmapData("0", i.Key, CCBeatmapData.DifficultyType.ExpertPlus, CCBeatmapData.Category.Special, 0, 0)).ToList()
                }
            }
        }.Use();
    }
}