using CompCube_Models.Models.ClientData;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.EnergyBar;
using CompCube.Utils;
using Reactive;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateView : ReactiveComponent
{
    private CustomEnergyBar _redHealthBar = null!;
    private CustomEnergyBar _blueHealthBar = null!;

    private State<float> _multiplierText = null!;
    private State<string> _statusText = null!;

    public float Multiplier
    {
        get => _multiplierText.Value;
        set => _multiplierText.Value = value;
    }
    
    public string StatusText
    {
        get => _statusText.Value;
        set => _statusText.Value = value;
    }

    public UserProfile RedProfile
    {
        get => _redPlayer.Value;
        set => _redPlayer.Value = value;
    }

    public UserProfile BlueProfile
    {
        get => _bluePlayer.Value;
        set => _bluePlayer.Value = value;
    }

    private State<UserProfile> _redPlayer = null!;
    private State<UserProfile> _bluePlayer = null!;

    public float RedHealth
    {
        get => _redHealthBar.Health;
        set => _redHealthBar.Health = value;
    }

    public float BlueHealth
    {
        get => _blueHealthBar.Health;
        set => _blueHealthBar.Health = value;
    }
    
    protected override GameObject Construct()
    {
        _multiplierText = Remember(0f);
        _statusText = Remember("Default");
        
        _redPlayer = Remember(new UserProfile("0", "default", "0", "", false, [], new(0, 0, 0, 0, 0, 0, 0)));
        _bluePlayer = Remember(new UserProfile("0", "default", "0", "", false, [], new(0, 0, 0, 0, 0, 0, 0)));
        
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Column,
                Gap = 10,
                AlignContent = Align.FlexEnd,
                JustifyContent = Justify.Center,
            },
            Children =
            {
                new Label()
                {
                    sText = _statusText.Map(x => x),
                    RichText = true,
                    FontStyle = FontStyles.Italic,
                    FontSize = 5,
                    Alignment = TextAlignmentOptions.Center
                },
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
                        JustifyContent = Justify.Center,
                        AlignItems = Align.FlexEnd,
                        AlignContent = Align.Center,
                        Gap = new YogaVector(15f, 0),
                    },
                    
                    Children =
                    {
                        new CustomEnergyBar(false, ColorConstants.RedTeamColor)
                        {
                            sPlayerName = _redPlayer.Map(x => x.GetFormattedUserName()),
                            sAvatarSource = _redPlayer.Map(x => x.AvatarUrl),
                        }.Bind(ref _redHealthBar),
                        
                        new Layout()
                        {
                            FlexController =
                            {
                                FlexDirection = FlexDirection.Column,
                                JustifyContent = Justify.Center,
                            },
                            
                            Children =
                            {
                                new Label()
                                {
                                    Text = "Damage Multiplier",
                                    FontStyle = FontStyles.Italic,
                                    FontSize = 3,
                                    Alignment = TextAlignmentOptions.Midline
                                },
                                new Label()
                                {
                                    sText = _multiplierText.Map(x => $"{x:F1}x"),
                                    FontStyle = FontStyles.Italic,
                                    Alignment = TextAlignmentOptions.Midline,
                                    FontSize = 7,
                                },
                            }
                        },
                        new CustomEnergyBar(true, ColorConstants.BlueTeamColor)
                        {
                            sPlayerName = _bluePlayer.Map(x => x.GetFormattedUserName()),
                            sAvatarSource = _bluePlayer.Map(x => x.AvatarUrl),
                            Health = 1f
                        }.Bind(ref _blueHealthBar),
                    }
                }
            }
        }.Use();
    }
}