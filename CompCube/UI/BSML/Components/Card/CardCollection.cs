using CompCube_Models.Models.Map;
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

                    sItems = _data.Map(x => x),

                    ConstructCell = context => new Card()
                    {
                        FlexItem =
                        {
                            sMargin = _marginSize.Map(x => new YogaFrame(0, Math.Min(0f, x)))
                        }

                        // Image = context.Item.GetBeatmapLevel()!.previewMediaData.GetCoverSpriteAsync().Result,
                        // Text = context.Item.MapCategory.ToString(),
                        // Color = context.Item.MapCategory.GetColorFromCategory(),
                        // Interactable = true
                    }
                }
            }
        }.Use();
    }
}