using System.Collections;
using BeatSaberMarkupLanguage;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;
using Background = Reactive.BeatSaber.Components.Background;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Match.Modal;

public class PhasePopupModal : ReactiveComponent
{
    [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
    
    public string Text
    {
        get => _text.Value;
        set => _text.Value = value;
    }

    public string SubText
    {
        get => _text.Value;
        set => _text.Value = value;
    }

    private State<string> _text = null!;
    private State<string> _subText = null!;

    private Reactive.BeatSaber.Components.Modal _modal = null!;
    
    protected override GameObject Construct()
    {
        _text = Remember(Text);
        _subText = Remember(SubText);
        
        _modal = new Reactive.BeatSaber.Components.Modal()
        {
            FlexItem =
            {
                Size = new YogaVector(50.pt, 35.pt)
            },
            
            Animated = true,

            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
            },

            Children =
            {
                new Background()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Column,
                        AlignContent = Align.Center,
                        JustifyContent = Justify.Center,
                        Gap = 3
                    },
                    FlexItem =
                    {
                        MinSize = new YogaVector()
                        {
                            x = 50,
                            y = 35
                        }
                    },
                    
                    Children =
                    {
                        new Label()
                        {
                            sText = _text.Map(x => x),
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Midline
                        },
                        new Label()
                        {
                            sText = _subText.Map(x => x),
                        }
                    }
                }.AsBlurBackground()
            }
        };

        return _modal.Use();
    }

    public static void CreateOnTransform(Transform transform, string text, string subText = "")
    {
        var modal = BeatSaberUI.DiContainer.Instantiate<PhasePopupModal>();
        
        modal.Use(transform);

        modal._sharedCoroutineStarter.Run(modal.ShowForSecondsAndHide());
    }

    private IEnumerator ShowForSecondsAndHide()
    {
        _modal.IsPushed = true;

        yield return new WaitForSeconds(2f);
        
        _modal.IsPushed = false;
    }
}

