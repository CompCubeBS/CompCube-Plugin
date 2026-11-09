using CompCube.Game.MatchState;
using CompCube.UI.BSML.Components.EnergyBar;
using CompCube.Utils;
using Reactive;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;
using AnimationCurve = UnityEngine.AnimationCurve;

namespace CompCube.UI.BSML.Match;

public class BottomScreenMatchStateView(MatchStateManager matchStateManager) : ReactiveComponent
{
    private Label _roundTextLabel = null!;
    
    protected override GameObject Construct()
    {
        _roundTextLabel = new Label()
        {
            IgnoreLayout = true,
            FlexItem =
            {
                Position = new YogaFrame(-10, 0)
            },
            Text = "Discard Phase",
            RichText = true,
            FontStyle = FontStyles.Italic,
            FontSize = 5,
            Alignment = TextAlignmentOptions.Center
        };
        
        return new Layout()
        {
            Children =
            {
                _roundTextLabel,
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
                        JustifyContent = Justify.Center,
                        AlignItems = Align.FlexEnd,
                        AlignContent = Align.Center,
                        Gap = new YogaVector(30f, 0),
                    },
                    
                    Children =
                    {
                        new CustomEnergyBar(matchStateManager.RedPlayer, ColorConstants.RedTeamColor, false),
                        new CustomEnergyBar(matchStateManager.BluePlayer, ColorConstants.BlueTeamColor, true)
                    }
                }
            }
        }.Use();
    }
}