using CompCube_Models.Models.Map;
using UnityEngine;

namespace CompCube.Extensions;

public static class CCBeatmapExtensions
{
    public static Color GetColorFromCategory(this CCBeatmapData.Category category) => category switch
    {
        CCBeatmapData.Category.Accuracy => new Color(0.204f, 0.596f, 0.859f, 1.0f),
        CCBeatmapData.Category.MidSpeed => Color.yellow,
        CCBeatmapData.Category.Tech => Color.magenta,
        CCBeatmapData.Category.Speed => new Color(1f, .5f, 0f),
        CCBeatmapData.Category.Extreme => Color.red,
        CCBeatmapData.Category.Special => Color.green,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };
}