using CompCube_Models.Models.Map;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Align = Reactive.Yoga.Align;
using Background = Reactive.BeatSaber.Components.Background;
using FlexDirection = Reactive.Yoga.FlexDirection;
using Image = Reactive.BeatSaber.Components.Image;
using Justify = Reactive.Yoga.Justify;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Components.Card;

public class MapCard : ReactiveComponent
{
    private const float ScaleModifier = 2.5f;
    
    public Sprite Image
    {
        get => _image.Value;
        set => _image.Value = value;
    }

    public Color Color
    {
        get => _color.Value;
        set => _color.Value = value;
    }

    public bool Interactable
    {
        get => _interactable.Value;
        set => _interactable.Value = value;
    }

    private State<string> _text = null!;
    private State<Sprite> _image = null!;
    private State<Color> _color = null!;
    private State<bool> _interactable = null!;

    protected override GameObject Construct()
    {
        _image = Remember(null as Sprite);
        _color = Remember(Color.red);
        _interactable = Remember(true);
        _text = Remember("special");
        
        return new Background()
        {
            FlexController =
            {
                ConstrainHorizontal = true,
                ConstrainVertical = true,
                
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center
            },
            
            FlexItem =
            {
                Size = new YogaVector(5 * ScaleModifier, 7 * ScaleModifier),
                MaxSize = new YogaVector(5 * ScaleModifier, 7 * ScaleModifier)
            },
            
            Sprite = BeatSaberResources.Sprites.background,
            
            PixelsPerUnit = 10,
            
            sColor = _color.Map(x => x with { a = 1f }),
            
            Children =
            {
                new Background()
                {
                    FlexController =
                    {
                        ConstrainHorizontal = true,
                        ConstrainVertical = true,
                        
                        AlignContent = Align.Center,
                        AlignItems = Align.Center,
                        JustifyContent = Justify.Center,
                        
                        FlexDirection = FlexDirection.Column,
                        
                        Gap = 2
                    },
                    
                    FlexItem =
                    {
                        Size = new YogaVector(5 * ScaleModifier, 7 * ScaleModifier),
                        MaxSize = new YogaVector(4.7f * ScaleModifier, 6.7f * ScaleModifier),
                    },
                    
                    PixelsPerUnit = 10,
                    
                    Sprite = BeatSaberResources.Sprites.background,
                    
                    Color = new Color32(20, 20, 20, 255),
                    
                    Children =
                    {
                        new Image()
                        {
                            sSprite = _image.Map(x =>
                            {
                                if (x is null)
                                {
                                    var texture = TextureHelpers.GetTextureFromResource("CompCube.Resources.compcube.png");
                            
                                    return Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                                }

                                return x;
                            }),
                    
                            PreserveAspect = true,
                    
                            Material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlowRoundEdge"),
                    
                            FlexItem =
                            {
                                MaxSize = new YogaVector(4 * ScaleModifier, 4 * ScaleModifier)
                            }
                        },
                        
                        new Background()
                        {
                            FlexItem =
                            {
                                Size = new YogaVector(3.75f * ScaleModifier, 1 * ScaleModifier),
                            },
                            
                            PixelsPerUnit = 30,
                            Sprite = BeatSaberResources.Sprites.background,
                            
                            sColor = _color.Map(x => x with { a = .6f }),
                            
                            Children =
                            {
                                new Label()
                                {
                                    sText = _text.Map(x => x.ToUpperInvariant()),
                                    Color = Color.white,
                                    FontStyle = FontStyles.Italic,
                                    FontSize = 2.75f,
                                    Alignment = TextAlignmentOptions.Midline
                                }
                            }
                        }
                    }
                }
            }
        }.Use();
    }
}