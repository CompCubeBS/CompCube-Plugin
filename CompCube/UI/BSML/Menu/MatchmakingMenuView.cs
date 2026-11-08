using CompCube.UI.BSML.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace CompCube.UI.BSML.Menu;

public class MatchmakingMenuView : ReactiveComponent
{
    public List<QueueOptionTab> _optionTabs = [new QueueOptionTab("Competitive", "standard_competitive_1v1")];

    private BsPrimaryTextButton _findMatchButton = null!;
    
    private BsTextButton _cancelButton = new()
    {
        Text = "Cancel",
        Enabled = false,
        FlexItem =
        {
            Size = new YogaVector(24, 8)
        }
    };
    
    public event Action? OnAboutButtonPressed;
    
    protected override GameObject Construct()
    {
        _findMatchButton = new BsPrimaryTextButton()
        {
            Text = "Find Match",
            RichText = true,
            Interactable = true,
            OnClick = () =>
            {
                ChangeState(true);
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
                AlignItems = Align.Center
            },

            Children =
            {
                _findMatchButton,
                _cancelButton
            }
        }.Use();
    }

    private void ChangeState(bool isConnected)
    {
        _findMatchButton.Interactable = !isConnected;
        _cancelButton.Enabled = isConnected;
    }
}