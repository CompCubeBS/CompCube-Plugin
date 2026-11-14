using CompCube_Models.Models.Map;
using CompCube.Extensions;
using Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CompCube.UI.BSML.Components.CustomLevelBar;

public class BsLevelBar : ReactiveComponent
{
    private LevelBar _levelBar = null!;

    protected override GameObject Construct()
    {
        var gameObject = new GameObject("LevelBar");
        
        _levelBar = Object.Instantiate(Resources.FindObjectsOfTypeAll<ResultsViewController>().First()._levelBar, gameObject.transform);

        return gameObject;
    }

    public void Setup(VotingMap map)
    {
        _levelBar.Setup(map.GetBeatmapLevel(), map.GetBaseGameDifficultyType(),
            map.GetBeatmapLevel()?.GetCharacteristics().First(i => i.serializedName == "Standard"));
    }
}