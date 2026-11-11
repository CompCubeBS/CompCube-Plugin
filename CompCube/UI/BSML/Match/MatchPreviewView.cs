using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;
using Label = Reactive.BeatSaber.Components.Label;

namespace CompCube.UI.BSML.Match;

public class MatchPreviewView : ReactiveComponent
{
    public UserProfile RedPlayer
    {
        get => _redPlayer.Value;
        set => _redPlayer.Value = value;
    }

    public UserProfile BluePlayer
    {
        get => _bluePlayer.Value;
        set => _bluePlayer.Value = value;
    }

    private State<UserProfile> _redPlayer = null!;
    private State<UserProfile> _bluePlayer = null!;

    protected override GameObject Construct()
    {
        _redPlayer = new State<UserProfile>(Constants.EmptyUser);
        _bluePlayer = new State<UserProfile>(Constants.EmptyUser);
        
        return new Layout()
        {
            FlexController =
            {
                AlignItems = Align.Center,
            },
            
            Children =
            {
                new Background()
                {
                    FlexItem =
                    {
                        // Position = new YogaFrame(-100.pt, -100.pt),
                        PositionType = PositionType.Absolute,
                        Position = new YogaFrame(15, 15),
                        Size = new YogaVector()
                        {
                            y = 70
                        }
                    },
                    
                    FlexController =
                    {
                        AlignItems = Align.FlexStart,
                        JustifyContent = Justify.FlexStart,
                    },
                    
                    Color = new Color(.5f, .5f, .5f),
                    
                    Children =
                    {
                        CreateUserProfile(_redPlayer, false),
                    }
                }
            }
        }.Use();
    }

    [StateGen]
    private Layout CreateUserProfile(State<UserProfile> player, bool reverseAlign)
    {
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = reverseAlign ? FlexDirection.RowReverse : FlexDirection.Row,
            },
            
            Children =
            {
                new WebImage()
                {
                    sSrc = player.Map(x => x.AvatarUrl),
                    Material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlowRoundEdge"),
                    PreserveAspect = true,
                    FlexItem =
                    {
                        Size = 10
                    }
                },
                new Label()
                {
                    sText = player.Map(x =>
                    {
                        if (reverseAlign)
                            return FormatElo(x.CompetitiveStats.Elo) + " " + x.GetFormattedUserName();
                        
                        return x.GetFormattedUserName() + " " + FormatElo(x.CompetitiveStats.Elo); 
                        

                        string FormatElo(int elo)
                        {
                            return $"<size=50%>({elo} ELO)</size>";
                        }
                    }),
                    RichText = true,
                    FontStyle = FontStyles.Italic,
                    FontSize = 10f,
                    Alignment = !reverseAlign ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.BottomRight,
                }
            }
        };
    }
}