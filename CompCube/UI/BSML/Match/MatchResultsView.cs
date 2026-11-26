using CompCube.Extensions;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Match;

public class MatchResultsView : ReactiveComponent
{
    public bool Won
    {
        get => _won.Value;
        set => _won.Value = value;
    }

    public int EloTransfer
    {
        get => _eloTransfer.Value;
        set => _eloTransfer.Value = value;
    }

    private State<bool> _won = null!;
    private State<int> _eloTransfer = null!;
    
    public event Action? OnContinueButtonPressed;
    
    protected override GameObject Construct()
    {
        _won = Remember(false);
        _eloTransfer = Remember(0);

        var text = RememberDerived(_won, _eloTransfer,
            (won, elo) =>
                $"You {(won ? "gained" : "lost")}: {Math.Abs(elo).ToString().FormatWithHtmlColor(won ? "#90EE90" : "#FF7F7F")} ELO");
        
        return new Layout()
        {
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                
                FlexDirection = FlexDirection.Column,
                
                Gap = 15f
            },
            
            Children =
            {
                new Background()
                {
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 100,
                            y = 12
                        }
                    },
                    
                    Sprite = BeatSaberResources.Sprites.background,
                    
                    PixelsPerUnit = 15,
                    
                    sGradientColor0 = _won.Map(x => x ? Constants.WinningBgColor : Constants.LosingBgColor),
                    GradientColor1 = new Color(0f, 0f, 0f, 0f),
                    UseGradient = true,
                    
                    Children =
                    {
                        new Label()
                        {
                            Alignment = TextAlignmentOptions.Midline,
                            sText = _won.Map(x => x ? "VICTORY!" : "DEFEAT..."),
                            FontSize = 10,
                            FontStyle = FontStyles.Italic,
                        }
                    }
                },
                
                new Label()
                {
                    sText = text.Map(x => x),
                    RichText = true,
                    FontStyle = FontStyles.Italic,
                    Alignment = TextAlignmentOptions.Midline,
                },
                
                new BsPrimaryTextButton()
                {
                    Text = "CONTINUE",
                    
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 34,
                            y = 10
                        }
                    },
                    
                    OnClick = () => OnContinueButtonPressed?.Invoke()
                }
            }
        }.Use();
    }
}