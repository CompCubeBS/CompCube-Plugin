using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Compiler;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;

namespace CompCube.UI.BSML.Match;

public class RoundResultsView : ReactiveComponent
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

    private State<bool> _didWin = null!;

    private BsLevelBar _bsLevelBar = null!;

    private State<MatchScore> _winnerMatchScore = null!;
    private State<MatchScore> _loserMatchScore = null!;
    
    protected override GameObject Construct()
    {
        _didWin = Remember(true);
        
        return new Layout()
        {
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Column,
            },
            
            Children =
            {
                new BsLevelBar().Bind(ref _bsLevelBar),
                
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
                    
                    Sprite = BeatSaberResources.Sprites.backgroundRight,
                    
                    GradientColor0 = Constants.WinningBgColor,
                    GradientColor1 = new Color(0f, 0f, 0f, 0f),
                    UseGradient = true,
                    
                    Children =
                    {
                        new Label()
                        {
                            Alignment = TextAlignmentOptions.Midline,
                            Text = "RESULTS",
                            FontSize = 10
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
                    },
                    
                    Children =
                    {
                        CreateScoreText(_winnerMatchScore, 1),
                        CreateScoreText(_loserMatchScore, 2)
                    }
                }
            }
        }.Use();
    }

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