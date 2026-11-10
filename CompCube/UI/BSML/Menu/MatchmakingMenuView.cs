using CompCube.Configuration;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace CompCube.UI.BSML.Menu;

public class MatchmakingMenuView(PluginConfig pluginConfig) : ReactiveComponent
{
    public event Action? AboutButtonPressed;
    
    public event Action? CancelButtonPressed;
    
    public event Action<string>? OnFindMatchButtonPressed;

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
                
                
                new BsPrimaryTextButton()
                {
                    Text = "Find Match",
                    RichText = true,
                    sInteractable = _isConnected.Map(x => !x),
                    OnClick = () =>
                    {
                        OnFindMatchButtonPressed?.Invoke(_segmentedControl.Key);
                    },
                    FlexItem = {
                        Size = new YogaVector(40, 11)
                    }
                },
                
                new BsTextButton()
                {
                    Text = "Cancel",
                    sEnabled = _isConnected.Map(x => x),
                    FlexItem =
                    {
                        Size = new YogaVector(24, 8),
                    },
                    OnClick = () => CancelButtonPressed?.Invoke()
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
}