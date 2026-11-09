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
    
    public event Action? OnFindMatchButtonPressed;

    private readonly Dictionary<string, BsSegmentedControlItem> _optionTabs = new()
        {
            ["standard_competitive_1v1"] = new BsSegmentedControlItem()
            { 
                Text = "Competitive",
            }
        };

    private BsPrimaryTextButton _findMatchButton = null!;

    private BsTextButton _cancelButton = null!;

    private BsTextButton _aboutButton = null!;

    private BsSegmentedControl<string> _segmentedControl = null!;
    
    protected override GameObject Construct()
    {
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
                    Key = _optionTabs.First().Key
                }.Bind(ref _segmentedControl),
                
                
                new BsPrimaryTextButton()
                {
                    Text = "Find Match",
                    RichText = true,
                    Interactable = true,
                    OnClick = () =>
                    {
                        OnFindMatchButtonPressed?.Invoke();
                    },
                    FlexItem = {
                        Size = new YogaVector(40, 11)
                    }
                }.Bind(ref _findMatchButton),
                
                new BsTextButton()
                {
                    Text = "Cancel",
                    Enabled = false,
                    FlexItem =
                    {
                        Size = new YogaVector(24, 8),
                    },
                    OnClick = () => CancelButtonPressed?.Invoke()
                }.Bind(ref _cancelButton),
                
                
                
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
                        }.Bind(ref _aboutButton)
                    }
                }
            }
        }.Use();
    }

    public void ChangeState(bool isConnected)
    {
        _findMatchButton.Interactable = !isConnected;
        _cancelButton.Enabled = isConnected;
        _aboutButton.Interactable = !isConnected;
    }

    public string GetActiveQueueOptionState()
    {
        return _segmentedControl.Key;
    }
}