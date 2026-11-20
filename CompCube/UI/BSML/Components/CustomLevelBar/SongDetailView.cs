using System.Globalization;
using System.Reflection;
using CompCube_Models.Models.Map;
using CompCube.Extensions;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Image = Reactive.Components.Basic.Image;

namespace CompCube.UI.BSML.Components.CustomLevelBar;

public class SongDetailView : ReactiveComponent
{
    public CCBeatmapData? Beatmap
    {
        get => _beatmap.Value;
        set => _beatmap.Value = value;
    }
    
    private State<CCBeatmapData?> _beatmap = null!;
    
    protected override GameObject Construct()
    {
        _beatmap = Remember(null as CCBeatmapData);

        return new Layout()
        {
            FlexController = { 
                FlexDirection = FlexDirection.Column,
                AlignContent = Align.Center,
                JustifyContent = Justify.Center,
            },
            
            Children =
            {
                new BsLevelBar()
                {
                    sData = _beatmap.Map(x => x),

                    FlexItem =
                    {
                        Position = new YogaFrame()
                        {
                            left = -37.5f,
                            top = -20f
                        }
                    },
                },

                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
                        JustifyContent = Justify.Center,
                        Gap = 2f
                    },

                    Children =
                    {
                        CreateBeatmapStat(
                            x => TimeSpan.FromSeconds(x.GetBeatmapLevel()!.songDuration).ToString(@"mm\:ss"),
                            FindTextureByName("ClockIcon")),
                        CreateBeatmapStat(x => Mathf.RoundToInt(x.GetBeatmapLevel()!.beatsPerMinute).ToString(),
                            FindTextureByName("MetronomeIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[
                                (x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return (data.notesCount / x.GetBeatmapLevel()!.songDuration).ToString("F",
                                CultureInfo.InvariantCulture);
                        }, FindTextureByName("NPSIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[
                                (x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.noteJumpMovementSpeed.ToString("F", CultureInfo.InvariantCulture);
                        }, FindTextureByName("FastNotesIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[
                                (x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.notesCount.ToString();
                        }, FindTextureByName("GameNoteIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[
                                (x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.obstaclesCount.ToString();
                        }, FindTextureByName("ObstacleIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[
                                (x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];
                        
                            return data.bombsCount.ToString();
                        }, FindTextureByName("BombIcon")),
                    }
                },

                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Column,

                        AlignContent = Align.Center,
                        JustifyContent = Justify.Center,
                        
                        Gap = 2.5f
                    },

                    Children =
                    {
                        CreateBgText(_beatmap, false),
                        CreateBgText(_beatmap, true),
                    }
                },
            }
        }.Use();
    }
    
    private static Sprite FindTextureByName(string name)
    {
        var method = typeof(BeatSaberMarkupLanguage.Utilities).GetMethod("FindSpriteCached",
            BindingFlags.Static | BindingFlags.NonPublic);

        var sprite = method?.Invoke(null, [name]);
        
        return (sprite as Sprite)!;
    }
    
    [StateGen]
    private Layout CreateBeatmapStat(Func<CCBeatmapData?, string> dataToDisplay, Sprite image)
    {
        return new Layout()
        {
            FlexController =
            {
                
                AlignContent = Align.Center,
                AlignItems = Align.Center,
                JustifyContent = Justify.Center,
                FlexDirection = FlexDirection.Column,
                Gap = 3f
            },

            Children =
            {
                new Image()
                {
                    Sprite = image,
                    
                    Material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat,
                    PixelsPerUnit = 100f,
                    
                    FlexItem =
                    {
                        Size = 4.5f
                    }
                },
                
                new Label()
                {
                    sText = _beatmap.Map(x =>
                    {
                        if (x is null)
                            return "text";
                        
                        return dataToDisplay(x);
                    }),
                    
                    FontStyle = FontStyles.Italic,
                    Alignment = TextAlignmentOptions.Midline
                }
            }
        };
    }
    
    [StateGen]
    private Background CreateBgText(State<CCBeatmapData> beatmap, bool forCategory)
    {
        return new Background()
        {
            FlexItem =
            {
                Size = new YogaVector()
                {
                    x = 85,
                    y = 5
                }
            },
            
            FlexController =
            {
                AlignItems = Align.Center,
                JustifyContent =  Justify.Center,
            },
            
            Children =
            {
                new Label()
                {
                    Alignment = TextAlignmentOptions.Midline,
                    sText = beatmap.Map(x =>
                    {
                        if (x is null)
                            return "example".ToUpperInvariant();
                        
                        return forCategory
                            ? $"Category: {x.MapCategory.ToString()}".ToUpperInvariant()
                            : GetBeatmapDifficultyText(x.Difficulty).ToUpperInvariant();
                    }),
                    FontStyle = FontStyles.Italic,
                    FontSize = 3f,
                    Color = new Color(0f, 0.7f, 1f, .9f)
                }
            }
        }.AsBlurBackground();
    }
    
    private string GetBeatmapDifficultyText(CCBeatmapData.DifficultyType difficulty)
    {
        if (difficulty == CCBeatmapData.DifficultyType.ExpertPlus)
            return "Expert+";

        return difficulty.ToString();
    }
}