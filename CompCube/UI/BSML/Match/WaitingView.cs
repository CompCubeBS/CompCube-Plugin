using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Match;

public class WaitingView : ReactiveComponent
{
    public string Text
    {
        get => _text.Value;
        set => _text.Value = value;
    }
    
    private State<string> _text = null!;
    
    protected override GameObject Construct()
    {
        _text = Remember("");
        
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Row,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
            },

            Children =
            {
                new Spinner()
                {
                    FlexItem =
                    {
                        Size = 3
                    }
                },
                
                new Label()
                {
                    sText = _text.Map(x => x),
                    RichText = true,
                    Alignment = TextAlignmentOptions.Midline,
                    FontStyle = FontStyles.Italic
                }
            }
        }.Use();
    }
}