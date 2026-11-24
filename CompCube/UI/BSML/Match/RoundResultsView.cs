using System.Globalization;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;
using Reactive.BeatSaber.Components;
using AnimationCurve = Reactive.AnimationCurve;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.Match;

public class RoundResultsView(float damageTextAnimationTime) : ReactiveComponent
{
    public bool DidWin
    {
        get => _didWin.Value;
        set => _didWin.Value = value;
    }

    public MatchScore WinningScore
    {
        get => _winnerMatchScore.Value;
        set => _winnerMatchScore.Value = value;
    }

    public MatchScore LosingScore
    {
        get => _loserMatchScore.Value;
        set => _loserMatchScore.Value = value;
    }

    public CCBeatmapData MapPlayed
    {
        get => _mapPlayed.Value;
        set => _mapPlayed.Value = value;
    }

    public float Damage
    {
        get => _damage.Value;
        set
        {
            _damage.SetValueImmediate(0f);
            _damage.TargetValue = value;
        }
    }

    public bool ReplayButtonActive
    {
        get => _replayButtonActive.Value;
        set => _replayButtonActive.Value = value;
    }
    
    public bool ReplayButtonInteractable
    {
        get => _replayButtonInteractable.Value;
        set => _replayButtonInteractable.Value = value;
    }

    public Action? ReplayButtonClickedCallback { get; set; } = null;

    private State<bool> _didWin = null!;

    private BsLevelBar _bsLevelBar = null!;

    private State<MatchScore> _winnerMatchScore = null!;
    private State<MatchScore> _loserMatchScore = null!;

    private State<CCBeatmapData> _mapPlayed = null!;

    private AnimatedState<float> _damage = null!;

    private State<bool> _replayButtonActive = null!;
    private State<bool> _replayButtonInteractable = null!;
    
    protected override GameObject Construct()
    {
        _didWin = Remember(true);
        _winnerMatchScore = Remember(new MatchScore(Constants.EmptyUser, Score.Empty));
        _loserMatchScore = Remember(new MatchScore(Constants.EmptyUser, Score.Empty));
        _mapPlayed = Remember(null as CCBeatmapData);
        _damage = RememberAnimated(0f, damageTextAnimationTime.sec, AnimationCurve.EaseOutExpo);
        _replayButtonActive = Remember(true);
        _replayButtonInteractable = Remember(true);
        
        return new Layout()
        {
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Column,
                
                Gap = 2.5f
            },
            
            FlexItem =
            {
                Size = new YogaVector()
                {
                    y = 100
                }
            },
            
            // Sprite = BeatSaberResources.Sprites.background,
            // Color = new Color(0.5f, 0.5f, 0.5f, 1f),
            
            Children =
            {
                new BsLevelBar()
                {
                    FlexItem =
                    {
                        Position = new YogaFrame()
                        {
                            left = -85,
                            top = -17.5f
                        }
                    },
                    
                    sData = _mapPlayed.Map(x => x)
                }.Bind(ref _bsLevelBar),
                
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
                    
                    GradientColor0 = Constants.ResultsBgColor,
                    GradientColor1 = new Color(0f, 0f, 0f, 0f),
                    UseGradient = true,
                    
                    Children =
                    {
                        new Label()
                        {
                            Alignment = TextAlignmentOptions.Midline,
                            Text = "RESULTS",
                            FontSize = 10,
                            FontStyle = FontStyles.Italic,
                        }
                    }
                },
                
                new Layout()
                {
                    FlexController =
                    {
                        Gap = 1.5f,
                        AlignItems = Align.Center,
                        JustifyContent = Justify.Center,
                        FlexDirection = FlexDirection.Column,
                    },
                    
                    Children =
                    {
                        CreateScoreText(_winnerMatchScore, 1),
                        CreateScoreText(_loserMatchScore, 2)
                    }
                },
                
                new Background()
                {
                    FlexController =
                    {
                        AlignContent = Align.Center,
                        AlignItems = Align.Center,
                        JustifyContent = Justify.Center,
                        FlexDirection = FlexDirection.Column,
                        Gap = 0f,
                        Padding = 3f
                    },
                    
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 30,
                            y = 12.5f
                        }
                    },
                    
                    Children =
                    {
                        new Label()
                        {
                            Text = "Damage",
                            
                            Alignment = TextAlignmentOptions.Midline,
                            FontStyle = FontStyles.Italic,
                        },
                        new Label()
                        {
                            sText = _damage.Map(x => x.ToString("P", CultureInfo.InvariantCulture)),
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Midline,
                            FontSize = 8,
                        }
                    }
                }.AsBlurBackground(),
                
                new BsTextButton()
                {
                    Text = "REPLAY",
                    
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 34,
                            y = 10
                        }
                    },
                    
                    OnClick = () =>
                    {
                        ReplayButtonClickedCallback?.Invoke();
                    },
                    
                    sInteractable = _replayButtonInteractable.Map(x => x),
                    sEnabled = _replayButtonActive.Map(x => x)
                }
            }
        }.Use();
    }

    public void ResetDamageAmountImmediate() => _damage.SetValueImmediate(0f);

    [StateGen]
    private Label CreateScoreText(State<MatchScore> score, int placement)
    {
        return new Label()
        {
            sText = score.Map(x => FormatScore(x.Score, x.User, placement)),
            RichText = true,
            FontStyle = FontStyles.Italic,
            Alignment = TextAlignmentOptions.Midline,
        };
    }
    
    private string FormatScore(Score score, UserProfile user, int placement) =>
        $"{(placement)}. {user.GetFormattedUserName()} - " +
        $"{(score.RelativeScore * 100):F}% " +
        $"{(score.FullCombo ? "FC".FormatWithHtmlColor("#90EE90") : $"{score.Misses}x".FormatWithHtmlColor("#FF7F7F"))}" +
        $"{(score.ProMode ? " (PM)" : "")}";
}