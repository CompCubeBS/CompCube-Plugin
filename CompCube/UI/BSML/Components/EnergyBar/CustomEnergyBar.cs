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

public class CustomEnergyBar(UserProfile profile, Color color, bool depleteFromLeft) : ReactiveComponent
{
    private AnimatedState<float> _barLength;
    
    protected override GameObject Construct()
    {
        _barLength = RememberAnimated(1f, 300.ms, AnimationCurve.EaseInOutCubic);
        
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
                    },
                    
                    Children =
                    {
                        new WebImage()
                        {
                            Src = profile.AvatarUrl,
                            Material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlowRoundEdge"),
                            PreserveAspect = true,
                            FlexItem =
                            {
                                Size = 10
                            }
                        },
                        
                        new Label()
                        {
                            Text = profile.GetFormattedUserName(),
                            RichText = true,
                            Alignment = !depleteFromLeft ? TextAlignmentOptions.BaselineLeft :  TextAlignmentOptions.BaselineRight,
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
                                Size = new YogaVector()
                                {
                                    x = 48.pt,
                                    y = 2.pt
                                }
                            },
                            
                            ContentTransform =
                            {
                                slocalScale = _barLength.Map(x => new Vector3(x, 1f))
                            },
                            
                            Sprite = BeatSaberResources.Sprites.background,
                            Color = color,
                            PixelsPerUnit = 30f,
                        }
                    }
                }
            }
        }.Use();
    }

    public void SetHealth(float health) => _barLength.TargetValue = health;
}