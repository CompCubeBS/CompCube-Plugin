using CompCube.UI.BSML.Components;
using CompCube.UI.BSML.EarlyLeaveWarning;
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

    private YesOrNoDialogModal _competitiveIntegrityModal = null!;
    
    public event Action? OnAboutButtonPressed;
    
    protected override GameObject Construct()
    {
        _competitiveIntegrityModal = new YesOrNoDialogModal(
            "Competitive matches may take a long time. Only queue when you can finish the match and protect competitive integrity. Are you sure you want to continue?",
            true,
            () => ChangeState(true),
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
                AlignItems = Align.Center
            },

            Children =
            {
                _findMatchButton,
                _cancelButton,
                _competitiveIntegrityModal
            }
        }.Use();
    }

    private void ChangeState(bool isConnected)
    {
        _findMatchButton.Interactable = !isConnected;
        _cancelButton.Enabled = isConnected;
    }
}