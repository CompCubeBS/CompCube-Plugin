using BeatSaberMarkupLanguage;
using CompCube.UI.BSML.Match;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace CompCube.UI.Dev;

public class DevView : ReactiveComponent
{
    public Action<ViewController>? OnButtonPressedCallback { get; set; } = null!;
    
    protected override GameObject Construct()
    {
        var context = new ScrollContext();
        
        return new Layout()
        {
            FlexController =
            {
                AlignContent = Align.Center,
                AlignItems = Align.Stretch,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Row,
                Gap = 2f
            },
            
            FlexItem =
            {
                Size = 100f
            },

            Children =
            {
                new ScrollArea()
                {
                    FlexItem =
                    {
                        Flex = 1,
                        Size = 100f
                    },
                    
                    ScrollContext = context,
                    
                    ScrollContent = new Layout()
                    {
                        FlexController =
                        {
                            AlignContent = Align.Center,
                            AlignItems = Align.Center,
                            JustifyContent = Justify.Center,
                            FlexDirection = FlexDirection.Column,
                            ConstrainVertical = false
                        },
                        
                        FlexItem =
                        {
                            Size = 100f,
                        },
                        
                        Children =
                        {
                            CreateViewControllerButton("MatchPreview", BeatSaberUI.DiContainer.Resolve<MatchPreviewViewController>()),
                            CreateViewControllerButton("MatchResults", BeatSaberUI.DiContainer.Resolve<MatchResultsViewController>()),
                            CreateViewControllerButton("RoundResults", BeatSaberUI.DiContainer.Resolve<RoundResultsViewController>()),
                            CreateViewControllerButton("PickScreen",  BeatSaberUI.DiContainer.Resolve<MapPickingViewController>()),
                            CreateViewControllerButton("WaitingForMatchToStart", BeatSaberUI.DiContainer.Resolve<WaitingForMatchToStartViewController>()),
                            CreateViewControllerButton("WaitingView", BeatSaberUI.DiContainer.Resolve<WaitingViewController>()),
                        }
                    }
                },
                
                new Scrollbar()
                {
                    ScrollContext = context,
                }
            }
        }.Use();
    }

    private BsTextButton CreateViewControllerButton(string text, ViewController controller) => new BsTextButton()
    {
        Text = text,
        OnClick = () => OnButtonPressedCallback?.Invoke(controller),
        
        FlexItem =
        {
            Size = new YogaVector(30, 11)
        }
    };
}