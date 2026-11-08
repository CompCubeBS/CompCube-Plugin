using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace CompCube.UI.BSML.EarlyLeaveWarning;

public class YesOrNoDialogModal(string text, bool clickerinoOfferinoCloserino, Action yesButtonOnClick, Action noButtonOnClick) : ReactiveComponent
{
    public string Text { get; set; } = text;

    public bool ClickerinoOfferinoCloserino { get; set; } = clickerinoOfferinoCloserino;

    public Action? NoButtonOnClick { get; set; } = noButtonOnClick;

    public Action? YesButtonOnClick { get; set; } = yesButtonOnClick;

    private Dialog _dialogModal = null!;

    protected override GameObject Construct()
    {
        _dialogModal = new Dialog()
        {
            FlexItem =
            {
                Size = new YogaVector(70, 45)
            },
            Animated = true,
            CancelButton =
            {
                Text = "No",
                OnClick = () =>
                {
                    Hide();
                    NoButtonOnClick?.Invoke();
                },
            },
            OkButton =
            {
                Text = "Yes",
                OnClick = () =>
                {
                    Hide();
                    YesButtonOnClick?.Invoke();
                },
            },
            Children =
            {
                new Label()
                {
                    Text = Text,
                    RichText = true,
                    EnableWrapping = true
                }
            }
        };
        
        return new Layout()
        {
            Children =
            {
                _dialogModal
            }
        }.Use();
    }

    public void Show()
    {
        _dialogModal.IsPushed = true;
    }
    
    public void Hide()
    {
        _dialogModal.IsPushed = false;
    }
}