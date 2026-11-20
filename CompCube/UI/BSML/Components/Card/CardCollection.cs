using CompCube_Models.Models.Map;
using CompCube.Extensions;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;
using Background = Reactive.BeatSaber.Components.Background;

namespace CompCube.UI.BSML.Components.Card;

public class CardCollection : ReactiveComponent
{
    public Action<CCBeatmapData>? OnCardWasSelected { get; set; } = null;
    
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
    private AnimatedState<float> _marginSize = null!;

    private Repeater<CCBeatmapData, Card> _cardRepeater = null!;
    
    protected override GameObject Construct()
    {
        _data = Remember(new List<CCBeatmapData>());
        _interactable = Remember(SelectionMode.Single);

        _marginSize = RememberAnimated(-5f, 400.ms, AnimationCurve.EaseOutCubic);

        return new Background()
        {
            Do = x => x.WithPointerEvents(onEnter: _ => _marginSize.TargetValue = 4f,
                                                    onLeave: _ => _marginSize.TargetValue = -5f),
            
            Sprite = BeatSaberResources.Sprites.rectangle,
            Color = Color.clear,
            
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
            },
            
            Children =
            {
                new Repeater<CCBeatmapData, Card>()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
                        sGap = _marginSize.Map(x => new YogaVector(Math.Max(0, x), 0))
                    },

                    sSelectionMode = _interactable.Map(x => x),
                    
                    OnSelectedItemsChanged = x =>
                    {
                        if (x.Count != 0)
                        {
                            OnCardWasSelected?.Invoke(x.First());
                            return;
                        }
                        
                        
                    },

                    sItems = _data.Map(x => x),

                    ConstructCell = context => new Card()
                    {
                        FlexItem =
                        {
                            sMargin = _marginSize.Map(x => new YogaFrame(0, Math.Min(0f, x)))
                        },
                        
                        OnSelect = _ => context.Selected = !context.Selected,

                        // Image = context.Item.GetBeatmapLevel()!.previewMediaData.GetCoverSpriteAsync().Result,
                        sText = context.Map(x => x.Item.MapCategory.ToString()),
                        sColor = context.Map(x => x.Item.MapCategory.GetColorFromCategory()),
                        Interactable = true,
                    }
                }.Bind(ref _cardRepeater)
            }
        }.Use();
    }
}