using BeatSaberMarkupLanguage;
using CompCube.Game;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using SongCore;
using TMPro;
using UnityEngine;
using Zenject;
using Label = Reactive.Components.Basic.Label;

namespace CompCube.UI.BSML.EarlyLeaveWarning;

public class DownloadingBeatmapsModal : ReactiveComponent
{
    [Inject] private readonly BeatmapDownloader _beatmapDownloader = null!;

    public int BeatmapsToDownload { get; set; } = 0;

    private int BeatmapsDownloaded
    {
        get => _beatmapsDownloaded.Value;
        set => _beatmapsDownloaded.Value = value;
    }
    
    private State<int> _beatmapsDownloaded = null!;

    private Reactive.BeatSaber.Components.Modal _modal = null!;
    
    protected override GameObject Construct()
    {
        _beatmapsDownloaded = Remember(0);
        
        return new Reactive.BeatSaber.Components.Modal()
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
                            sText = _beatmapsDownloaded.Map(x => BeatmapsToDownload == 0 ? "Downloading beatmaps..." : $"Downloading beatmaps... ({x}/{BeatmapsToDownload})"),
                            FontStyle = FontStyles.Italic,
                            Alignment = TextAlignmentOptions.Midline
                        },
                        new Spinner()
                        {
                            FlexItem =
                            {
                                Size = 6,
                                AlignSelf = Align.Center,
                            }
                        }
                    }
                }.AsBlurBackground()
            }
        }.Bind(ref _modal).Use();
    }

    public static async Task CreateOnTransformAndDownloadBeatmaps(Transform transform, string[] hashes, Action? callBack = null)
    {
        hashes = hashes.Distinct().Where(i => !Collections.songWithHashPresent(i)).ToArray();

        if (hashes.Length == 0)
        {
            callBack?.Invoke();
            return;
        }

        var modal = BeatSaberUI.DiContainer.Instantiate<DownloadingBeatmapsModal>();

        modal.BeatmapsToDownload = hashes.Length;

        modal.Use(transform);

        modal.Show();

        await modal._beatmapDownloader.DownloadMaps(
            hashes, 
            (count) => modal.BeatmapsDownloaded = count,
            () =>
            {
                modal.Hide();
                callBack?.Invoke();
            });
    }

    private void Show()
    {
        _modal.IsPushed = true;
    }

    private void Hide()
    {
        _modal.IsPushed = false;
    }
}