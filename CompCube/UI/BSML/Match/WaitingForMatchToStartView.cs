using System.Globalization;
using System.Reflection;
using CompCube_Models.Models.Map;
using CompCube.Configuration;
using CompCube.Extensions;
using CompCube.UI.BSML.Components.CustomLevelBar;
using CompCube.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using SiraUtil.Extras;
using TMPro;
using UnityEngine;
using Zenject;
using Image = Reactive.Components.Basic.Image;

namespace CompCube.UI.BSML.Match;

public class WaitingForMatchToStartView : ReactiveComponent
{
    public CCBeatmapData Beatmap
    {
        get => _beatmap.Value;
        set => _beatmap.Value = value;
    }
    
    private State<CCBeatmapData> _beatmap = null!;

    public DateTime TimerEndTime { get; set; } = DateTime.Now;

    private State<float>? _secondsUntilTimerEnds = null;

    public event Action<bool>? OnScoreSubmissionToggled;
    
    protected override GameObject Construct()
    {
        _beatmap = Remember(null as CCBeatmapData);
        _secondsUntilTimerEnds = Remember(0f);

        var rail = new NamedRail();
        
        var layout = new Layout()
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
            
            Children =
            {
                new BsLevelBar()
                {
                    sData = _beatmap.Map(x => x),
                    
                    FlexItem =
                    {
                        Position = new YogaFrame()
                        {
                            left = -80,
                            top = -17.5f
                        }
                    },
                },
                
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Row,
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
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[(x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];
                            
                            return (data.notesCount / x.GetBeatmapLevel()!.songDuration).ToString("F", CultureInfo.InvariantCulture);
                        }, FindTextureByName("NPSIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[(x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.noteJumpMovementSpeed.ToString("F", CultureInfo.InvariantCulture);
                        }, FindTextureByName("FastNotesIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[(x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.notesCount.ToString();
                        }, FindTextureByName("GameNoteIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[(x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.obstaclesCount.ToString();
                        }, FindTextureByName("ObstacleIcon")),
                        CreateBeatmapStat(x =>
                        {
                            var data = x.GetBeatmapLevel()!.beatmapBasicData[(x.GetBeatmapKey().beatmapCharacteristic, x.GetBeatmapKey().difficulty)];

                            return data.bombsCount.ToString();
                        }, FindTextureByName("BombIcon")),
                    }
                },
                
                new Layout()
                {
                    FlexController =
                    {
                        FlexDirection = FlexDirection.Column,
                        
                        Gap = 2.5f
                    },
                    
                    Children =
                    {
                        CreateBgText(_beatmap, false),
                        CreateBgText(_beatmap, true),
                    }
                },
                
                new Layout()
                {
                    FlexItem =
                    {
                        Size = new YogaVector()
                        {
                            x = 85,
                            y = 7
                        }
                    },
                    
                    Children =
                    {
                        new BsToggle()
                        {
                            Animated = true,
                    
                            Toggled = true,
                    
                            OnToggleStateChanged = (value) => OnScoreSubmissionToggled?.Invoke(value),
                        }.InNamedRail("Score Submission").Bind(ref rail)
                    }
                },
                
                new Label()
                {
                    FontStyle = FontStyles.Italic,
                    sText = _secondsUntilTimerEnds.Map(x => $"Starting in {(((int) _secondsUntilTimerEnds.Value) + 1).ToString(CultureInfo.InvariantCulture)}...")
                }
            }
        }.Use();

        rail.Label.Alignment = TextAlignmentOptions.Midline;
        rail.Label.FontStyle = FontStyles.Italic;

        return layout;
    }
    
    private static Sprite FindTextureByName(string name)
    {
        var method = typeof(BeatSaberMarkupLanguage.Utilities).GetMethod("FindSpriteCached",
            BindingFlags.Static | BindingFlags.NonPublic);

        var sprite = method?.Invoke(null, [name]);
        
        return (sprite as Sprite)!;
    }

    protected override void OnUpdate()
    {
        _secondsUntilTimerEnds?.Value = (float) (TimerEndTime - DateTime.Now).TotalSeconds;
    }

    private string GetBeatmapDifficultyText(CCBeatmapData.DifficultyType difficulty)
    {
        if (difficulty == CCBeatmapData.DifficultyType.ExpertPlus)
            return "Expert+";

        return difficulty.ToString();
    }

    [StateGen]
    private Layout CreateBeatmapStat(Func<CCBeatmapData, string> dataToDisplay, Sprite image)
    {
        return new Layout()
        {
            FlexController =
            {
                FlexDirection = FlexDirection.Column,
                Gap = 1f
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
}