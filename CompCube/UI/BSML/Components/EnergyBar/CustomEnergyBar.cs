using System.Globalization;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.ViewControllers;
using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;
using Image = Reactive.Components.Basic.Image;
using Object = UnityEngine.Object;

namespace CompCube.UI.BSML.Components.EnergyBar;

public class CustomEnergyBar(bool depleteFromLeft, Color color) : ReactiveComponent
{
    public string PlayerName
    {
        get => _playerName.Value;
        set => _playerName.Value = value;
    }
    
    public string AvatarSource
    {
        get => _avatarSource.Value;
        set => _avatarSource.Value = value;
    }
    
    public float Health
    {
        get => _barLength.TargetValue;
        set => _barLength.TargetValue = value;
    }
    
    private AnimatedState<float> _barLength = null!;

    private State<string> _avatarSource = null!;
    private State<string> _playerName = null!;
    
    protected override GameObject Construct()
    {
        _avatarSource = Remember("https://cdn.scoresaber.com/avatars/oculus.png");
        _playerName = Remember("default");
        
        _barLength = RememberAnimated(1f, 300.ms, AnimationCurve.EaseOutCubic);
        
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Column,
            },
            
            Children =
            {
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = !depleteFromLeft ? FlexDirection.Row : FlexDirection.RowReverse,
                        Gap = new YogaVector()
                        {
                            x = 1.5f
                        }
                    },
                    
                    Children =
                    {
                        new WebImage()
                        {
                            sSrc = _avatarSource.Map(x => x),
                            Material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlowRoundEdge"),
                            PreserveAspect = true,
                            FlexItem =
                            {
                                Size = 7.5f
                            }
                        },
                        
                        new Label()
                        {
                            sText = _playerName.Map(x => x),
                            RichText = true,
                            Alignment = !depleteFromLeft ? TextAlignmentOptions.BottomLeft :  TextAlignmentOptions.BottomRight,
                            FontStyle = FontStyles.Italic
                        },
                    }
                },
                
                new Background()
                {
                    FlexController =
                    {
                        AlignItems = Align.Center,
                        Padding = 1.pt
                    },
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 50.pt,
                            y = 4.pt
                        }
                    },
                    Sprite = BeatSaberResources.Sprites.background,
                    PixelsPerUnit = 30f,
                    Color = new Color(0f, 0f, 0f, .35f),
            
                    Children =
                    {
                        new Background()
                        {
                            FlexController =
                            {
                                JustifyContent = Justify.Center,
                                AlignItems = Align.Center,
                            },
                    
                            FlexItem =
                            {
                                sSize = _barLength.Map(x => new YogaVector((48 * x).pt, 2.pt)),
                                sPosition = _barLength.Map(x => new YogaFrame()
                                {
                                    right = !depleteFromLeft ? 0 : new YogaValue((x * 48) - 48, Unit.Point)
                                })
                            },
                            
                            Sprite = BeatSaberResources.Sprites.background,
                            Color = color,
                            PixelsPerUnit = 30f,
                        }
                    }
                },
                new Label()
                {
                    sText = _barLength.Map(x => x.ToString("P", CultureInfo.InvariantCulture)),
                    Color = color,
                    Alignment = !depleteFromLeft ? TextAlignmentOptions.BottomLeft :  TextAlignmentOptions.BottomRight,
                    FontStyle = FontStyles.Italic,
                    FontSize = 4f
                },
            }
        }.Use();
    }
}