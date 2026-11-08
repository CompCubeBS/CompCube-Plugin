using CompCube.Configuration;
using CompCube.Interfaces;
using CompCube.UI.BSML.Components;
using CompCube.UI.BSML.EarlyLeaveWarning;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Menu;

public class MatchmakingMenuView(PluginConfig pluginConfig) : ReactiveComponent
{
    public event Action? AboutButtonPressed;
    
    public event Action? CancelButtonPressed;
    
    public event Action? ShouldFindMatch;

    private readonly Dictionary<QueueOptionTab, BsSegmentedControlItem> _optionTabs = new()
    {
        {
            new QueueOptionTab("Competitive", "standard_competitive_1v1"),
            new BsSegmentedControlItem()
            {
                Text = "Competitive",
            }
        }
    };

    private BsPrimaryTextButton _findMatchButton = null!;

    private BsTextButton _cancelButton = null!;

    private BsTextButton _aboutButton = null!;

    private YesOrNoDialogModal _competitiveIntegrityModal = null!;

    private BsSegmentedControl<QueueOptionTab> _segmentedControl = null!;
    
    protected override GameObject Construct()
    {
        _segmentedControl = new BsSegmentedControl<QueueOptionTab>
        {
            FlexItem =
            {
                AlignSelf = Align.Center
            },
            Skew = .1f,
            Items = _optionTabs,
            Key = _optionTabs.First().Key
        };

        if (pluginConfig.ShowDebugQueue)
        {
            _optionTabs.Add(new QueueOptionTab("debug", "debug"), new BsSegmentedControlItem()
            {
                Text = "Debug",
            });

            _segmentedControl.Items = _optionTabs;
        }

        _aboutButton = new BsTextButton()
        {
            FlexItem =
            {
                Size = new YogaVector(24, 8),
                // PositionType = PositionType.Absolute,
                // Position = new YogaFrame(0, -10)
            },

            Text = "About",
            OnClick = () => AboutButtonPressed?.Invoke(),
        };
        
        _cancelButton = new()
        {
            Text = "Cancel",
            Enabled = false,
            FlexItem =
            {
                Size = new YogaVector(24, 8),
            },
            OnClick = () => CancelButtonPressed?.Invoke()
        };
        
        _competitiveIntegrityModal = new YesOrNoDialogModal(
            "Competitive matches may take a long time. Only queue when you can finish the match and protect competitive integrity. Are you sure you want to continue?",
            true,
            () =>
            {
                ChangeState(true);
                ShouldFindMatch?.Invoke();
            },
            () => {}
        );
        
        _findMatchButton = new BsPrimaryTextButton()
        {
            Text = "Find Match",
            RichText = true,
            Interactable = true,
            OnClick = () =>
            {
                _competitiveIntegrityModal.Show();
            },
            FlexItem = {
                Size = new YogaVector(40, 11)
            }
        };
        
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
                _segmentedControl,
                _findMatchButton,
                _cancelButton,
                _competitiveIntegrityModal,
                
                new Layout()
                {
                    IgnoreLayout = true,
                    
                    Children =
                    {
                        _aboutButton
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

    public QueueOptionTab GetActiveQueueOptionState()
    {
        return _segmentedControl.Key;
    }
}