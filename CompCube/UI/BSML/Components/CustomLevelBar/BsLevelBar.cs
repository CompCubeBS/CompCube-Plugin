using CompCube_Models.Models.Map;
using CompCube.Extensions;
using Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CompCube.UI.BSML.Components.CustomLevelBar;

public class BsLevelBar : ReactiveComponent
{
    private LevelBar _levelBar = null!;

    public VotingMap? Data
    {
        get => _data.Value;
        set => _data.Value = value;
    }

    private State<VotingMap> _data = null!;
    
    protected override GameObject Construct()
    {
        _data = Remember(null as VotingMap);
        
        var gameObject = new GameObject("LevelBar");
        
        _levelBar = Object.Instantiate(Resources.FindObjectsOfTypeAll<ResultsViewController>().First()._levelBar, gameObject.transform);

        _data.AddCallback(HandleStateChange);

        return gameObject;
    }

    private void HandleStateChange(VotingMap? data)
    {
        if (data is null)
        {
            _levelBar._songNameText.text = "Empty";
            return;
        }
            
        _levelBar.Setup(data.GetBeatmapLevel(), data.GetBaseGameDifficultyType(),
            data.GetBeatmapLevel()?.GetCharacteristics().First(i => i.serializedName == "Standard"));
    }
}