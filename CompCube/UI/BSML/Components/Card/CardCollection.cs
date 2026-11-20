using CompCube_Models.Models.Map;
using CompCube.Extensions;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using SongCore;
using UnityEngine;

namespace CompCube.UI.BSML.Components.Card;

public class CardCollection : ReactiveComponent
{
    public List<CCBeatmapData> Data
    {
        get => _data.Value;
        set => _data.Value = value;
    }

    public bool Interactable
    {
        get => _interactable.Value == SelectionMode.Single;
        set => _interactable.Value = value ? SelectionMode.Single : SelectionMode.None;
    }

    private State<List<CCBeatmapData>> _data = null!;
    private State<SelectionMode> _interactable = null!;
    
    protected override GameObject Construct()
    {
        _data = Remember(new List<CCBeatmapData>());
        _interactable = Remember(SelectionMode.Single);
        
        return new Repeater<CCBeatmapData, Card>()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Row,
                Gap = 4
            },
            
            sSelectionMode = _interactable.Map(x => x),
            
            
            
            sItems = _data.Map(x => x),
            
            ConstructCell = context => new Card()
            {
                // Image = context.Item.GetBeatmapLevel()!.previewMediaData.GetCoverSpriteAsync().Result,
                // Text = context.Item.MapCategory.ToString(),
                // Color = context.Item.MapCategory.GetColorFromCategory(),
                // Interactable = true
            }
        }.Use();
    }
}