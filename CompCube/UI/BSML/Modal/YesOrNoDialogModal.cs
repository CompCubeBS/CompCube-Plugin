using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace CompCube.UI.BSML.EarlyLeaveWarning;

public class YesOrNoDialogModal(string text, bool clickerinoOfferinoCloserino, Action yesButtonOnClick, Action noButtonOnClick) : ReactiveComponent
{
    private Dialog _dialogModal = null!;

    protected override GameObject Construct()
    {
        _dialogModal = new Dialog()
        {
            FlexController =
            {
                Padding = 3f,
                ConstrainHorizontal = true
            },
            FlexItem =
            {
                Size = new YogaVector(70, 45),
                MaxSize = new YogaVector(70, 45)
            },
            Animated = true,
            CancelButton =
            {
                Text = "No",
                FontStyle = FontStyles.Italic,
                OnClick = () =>
                {
                    Hide();
                    noButtonOnClick?.Invoke();
                },
            },
            OnClickOutside = () =>
            {
                if (!clickerinoOfferinoCloserino)
                    return;

                _dialogModal.IsPushed = false;
            },
            OkButton =
            {
                Text = "Yes",
                FontStyle = FontStyles.Italic,
                OnClick = () =>
                {
                    Hide();
                    yesButtonOnClick?.Invoke();
                },
            },
            Children =
            {
                new Label()
                {
                    Text = text,
                    RichText = true,
                    EnableWrapping = true,
                    FontStyle = FontStyles.Italic,
                    FlexItem = { MaxSize = new YogaVector(70, 45) }
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