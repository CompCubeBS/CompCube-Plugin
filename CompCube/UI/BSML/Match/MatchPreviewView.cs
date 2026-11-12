using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.Utils;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;
using AnimationCurve = Reactive.AnimationCurve;
using Background = Reactive.BeatSaber.Components.Background;
using Image = UnityEngine.UI.Image;
using Label = Reactive.BeatSaber.Components.Label;

namespace CompCube.UI.BSML.Match;

public class MatchPreviewView : ReactiveComponent
{
    public UserProfile RedPlayer
    {
        get => _redPlayer.Value;
        set => _redPlayer.Value = value;
    }

    public UserProfile BluePlayer
    {
        get => _bluePlayer.Value;
        set => _bluePlayer.Value = value;
    }

    private State<UserProfile> _redPlayer = null!;
    private State<UserProfile> _bluePlayer = null!;

    private AnimatedState<float> _barAnimationState = null!;

    // private AnimationPlayer<> _settingUpMatchAnimationState = null!;

    protected override GameObject Construct()
    {
        _redPlayer = new State<UserProfile>(Constants.EmptyUser);
        _bluePlayer = new State<UserProfile>(Constants.EmptyUser);

        _barAnimationState = RememberAnimated(0f, 5.sec, AnimationCurve.Linear);
        // _settingUpMatchAnimationState = RememberAnimationPlayer();
        
        return new Layout()
        {
            FlexController =
            {
                JustifyContent = Justify.Center,
                AlignItems = Align.Center,
            },
            
            Children =
            {
                new Label()
                {
                    FlexItem =
                    {
                        PositionType = PositionType.Absolute,
                        AlignSelf = Align.Center,
                    },
                    
                    Text = "vs.",
                    FontSize = 10f,
                    Alignment = TextAlignmentOptions.Midline,
                    FontStyle = FontStyles.Italic
                },
                
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
                        JustifyContent = Justify.Center,
                        AlignContent = Align.Center,
                        Gap = 1
                    },
                    
                    FlexItem =
                    {
                        Position = new YogaFrame(10f, 0)
                    },
                    
                    Children =
                    {
                        new Spinner()
                        {
                            RotationSpeed = 15,
                            FlexItem =
                            {
                                Size = 5f
                            }
                        },
                        
                        new Label()
                        {
                            Text = "Preparing match...",
                            FontSize = 5f,
                            Alignment = TextAlignmentOptions.MidlineLeft,
                            FontStyle = FontStyles.Italic
                        }
                    }
                },
                
                new Layout()
                {
                    FlexItem =
                    {
                        // Position = new YogaFrame(-100.pt, -100.pt),
                        PositionType = PositionType.Absolute,
                        Position = new YogaFrame(10, 25),
                        Size = new YogaVector()
                        {
                            y = 70
                        }
                    },
                    
                    FlexController =
                    {
                        AlignItems = Align.FlexStart,
                        JustifyContent = Justify.FlexStart,
                    },
                    
                    Children =
                    {
                        CreateUserProfile(_redPlayer, false, Constants.RedTeamColor),
                    }
                },
                
                new Layout()
                {
                    FlexItem =
                    {
                        PositionType = PositionType.Absolute,
                        Position = new YogaFrame(-5, 35),
                        Size = new YogaVector()
                        {
                            y = 70
                        }
                    },
                    
                    FlexController =
                    {
                        AlignItems = Align.FlexEnd,
                        JustifyContent = Justify.FlexEnd,
                    },
                    
                    Children =
                    {
                        CreateUserProfile(_bluePlayer, true, Constants.BlueTeamColor),
                    }
                }
            }
        }.Use();
    }

    public void StartAnimation()
    {
        _barAnimationState.SetValueImmediate(0f);
        _barAnimationState.TargetValue = 1f;
    }

    [StateGen]
    private Layout CreateUserProfile(State<UserProfile> player, bool reverseAlign, Color color)
    {
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Column,
                Gap = 5
            },
            
            FlexItem =
            {
                Size = new YogaVector(70, 10)
            },
            
            Children =
            {
                new Layout
                {
                    FlexController =
                    {
                        FlexDirection = reverseAlign
                            ? FlexDirection.RowReverse
                            : FlexDirection.Row,
                        Gap = 3,
                        AlignItems = Align.Center,
                    },
                    
                    FlexItem =
                    {
                        Size = new YogaVector(70, 5)
                    },

                    Children =
                    {
                        new WebImage()
                        {
                            sSrc = player.Map(x => x.AvatarUrl),
                            Material = Resources.FindObjectsOfTypeAll<Material>()
                                .FirstOrDefault(m => m.name == "UINoGlowRoundEdge"),
                            PreserveAspect = true,
                            FlexItem =
                            {
                                Size = 10
                            }
                        },
                        new Label()
                        {
                            sText = player.Map(x =>
                            {
                                if (reverseAlign)
                                    return FormatElo(x.CompetitiveStats.Elo) + " " + x.GetFormattedUserName();

                                return x.GetFormattedUserName() + " " + FormatElo(x.CompetitiveStats.Elo);


                                string FormatElo(int elo)
                                {
                                    return $"<size=50%>({elo} ELO)</size>";
                                }
                            }),
                            RichText = true,
                            FontStyle = FontStyles.Italic,
                            FontSize = 6.5f,
                            Alignment = !reverseAlign
                                ? TextAlignmentOptions.TopLeft
                                : TextAlignmentOptions.BottomRight,
                        }
                    }
                },
                
                new Background()
                {
                    GradientColor0 = reverseAlign ? Color.white with {a = 0f} : color with {a  = 0.6f},
                    GradientColor1 = !reverseAlign ? Color.white with {a = 0f} : color with {a  = 0.6f},
                    UseGradient = true,
                    
                    PixelsPerUnit = 30f,
                    Sprite = BeatSaberResources.Sprites.background,
                    
                    FlexItem =
                    {
                        sSize = _barAnimationState.Map(x => new YogaVector((70 * x).pt, 2.pt)),
                        sPosition = _barAnimationState.Map(x => new YogaFrame()
                        {
                            right = !reverseAlign ? 0 : new YogaValue((x * 70) - 70, Unit.Point)
                        })
                    }
                }
            }
        };
    }
}