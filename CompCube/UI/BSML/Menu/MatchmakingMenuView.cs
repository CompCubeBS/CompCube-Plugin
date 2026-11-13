using CompCube.Configuration;
using CompCube.UI.BSML.EarlyLeaveWarning;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace CompCube.UI.BSML.Menu;

public class MatchmakingMenuView(PluginConfig pluginConfig) : ReactiveComponent
{
    public event Action? AboutButtonPressed;
    
    public event Action? CancelButtonPressed;
    
    public event Action<string>? OnShouldConnectToServer;

    public bool IsConnected
    {
        get => _isConnected.Value;
        set => _isConnected.Value = value;
    }

    private readonly Dictionary<string, BsSegmentedControlItem> _optionTabs = new()
        {
            ["standard_competitive_1v1"] = new BsSegmentedControlItem()
            { 
                Text = "Competitive",
            }
        };
    
    private BsSegmentedControl<string> _segmentedControl = null!;

    private State<bool> _isConnected = null!;
    
    protected override GameObject Construct()
    {
        _isConnected = Remember(false);
        
        if (pluginConfig.ShowDebugQueue)
        {
            _optionTabs["debug"] = new BsSegmentedControlItem()
            {
                Text = "Debug"
            };
        }
        
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Column,
                AlignItems = Align.Center,
                Gap = new YogaVector(3f,3f)
            },

            Children =
            {
                new BsSegmentedControl<string>
                {
                    FlexItem =
                    {
                        AlignSelf = Align.Center
                    },
                    Skew = .1f,
                    Items = _optionTabs,
                    Key = _optionTabs.First().Key,
                    sInteractable = _isConnected.Map(x => !x)
                }.Bind(ref _segmentedControl),
                
                
                new BsPrimaryButton()
                {
                    
                    sInteractable = _isConnected.Map(x => !x),
                    OnClick = () => YesOrNoDialogModal.CreateOnTransform(ContentTransform, 
                        "Competitive matches may take a long time. Only queue when you can finish the match and protect competitive integrity. Are you sure you want to continue?",
                        true, () =>
                        {
                            _isConnected.Value = true;
                            OnShouldConnectToServer?.Invoke(_segmentedControl.Key);
                        },
                        () => { }),
                    FlexItem = {
                        Size = new YogaVector(40, 11)
                    },
                    
                    ConstructContent = (color, skew) => new Layout()
                    {
                        FlexController =
                        {
                            FlexDirection = FlexDirection.Row,
                            AlignContent = Align.Center,
                            JustifyContent = Justify.Center,
                            Gap = 1
                        },
                        
                        Children =
                        {
                            new Spinner()
                            {
                                sEnabled = _isConnected.Map(x => x),
                                FlexItem =
                                {
                                    Size = 3
                                }
                            },
                            
                            new Label()
                            {
                                sText = _isConnected.Map(x => x ? "FINDING MATCH..." : "FIND MATCH"),
                                RichText = true,
                                FontStyle = FontStyles.Italic
                            }
                        }
                    }
                },
                
                new BsAeroButton()
                {
                    sEnabled = _isConnected.Map(x => x),
                    FlexItem =
                    {
                        Size = new YogaVector(24, 8),
                    },
                    OnClick = () =>
                    {
                        CancelButtonPressed?.Invoke();
                    },

                    Colors = GetHoveredColorScheme(Color.red),
                    
                    ConstructContent = (color , skew) => new Label()
                    {
                        Text = "Cancel",
                    }
                },
                
                
                
                new Layout()
                {
                    IgnoreLayout = true,
                    
                    Children =
                    {
                        new BsTextButton()
                        {
                            FlexItem =
                            {
                                Size = new YogaVector(24, 8),
                                // PositionType = PositionType.Absolute,
                                // Position = new YogaFrame(0, -10)
                            },

                            Text = "About",
                            OnClick = () => AboutButtonPressed?.Invoke(),
                            sInteractable = _isConnected.Map(x => !x)
                        }
                    }
                }
            }
        }.Use();
    }

    private BsAeroButtonColors GetHoveredColorScheme(Color hovered)
    {
        var colors = BeatSaberStyle.BsAeroButtonColors;

        return colors with
        {
            BackgroundColors = colors.BackgroundColors with
            {
                Colors = colors.BackgroundColors.Colors with
                {
                    HoveredColor = hovered
                }
            }
        };
    }
}