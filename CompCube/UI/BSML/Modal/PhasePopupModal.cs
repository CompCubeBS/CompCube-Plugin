using System.Collections;
using BeatSaberMarkupLanguage;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;
using AnimationCurve = Reactive.AnimationCurve;
using Background = Reactive.BeatSaber.Components.Background;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Modal;

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
        get => _subText.Value;
        set => _subText.Value = value;
    }

    private State<string> _text = null!;
    private State<string> _subText = null!;

    private Reactive.BeatSaber.Components.Modal _modal = null!;
    
    protected override GameObject Construct()
    {
        _text = Remember("text");
        _subText = Remember("");
        
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
            
            OpenAnimation = new Animation<IReactiveComponent>(AnimationTracks.Fade(1f, 1f) + AnimationTracks.Scale(1f, 1f), 0.ms),
            CloseAnimation = new Animation<IReactiveComponent>(AnimationTracks.Fade(1f, 0f) + AnimationTracks.Scale(Vector3.one, new Vector3(.7f, .7f, 1f)), 200.ms, AnimationCurve.EaseOutCubic),

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
                            sText = _text.Map(x => $"<u>{x}</u>"),
                            RichText = true,
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Midline,
                            FontSize = 7,
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
        
        modal.Text = text;
        modal.SubText = subText;
        
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

