using CompCube_Models.Models.Map;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UIElements;
using Align = Reactive.Yoga.Align;
using Background = Reactive.Components.Basic.Background;
using Image = Reactive.BeatSaber.Components.Image;
using Justify = Reactive.Yoga.Justify;

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

    private State<Sprite> _image = null!;
    private State<Color> _color = null!;

    protected override GameObject Construct()
    {
        _image = Remember(null as Sprite);
        _color = Remember(new Color(1f, 1f, 1f, 0.6f));
        
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
            
            sColor = _color.Map(x => x),
            
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
                    
                    FlexItem =
                    {
                        MaxSize = new YogaVector(4 * ScaleModifier, 4 * ScaleModifier)
                    }
                }
            }
        }.Use();
    }
}