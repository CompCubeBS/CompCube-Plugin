using CompCube_Models.Models.ClientData;
using UnityEngine;

namespace CompCube.Utils;

public static class Constants
{
    public static Color RedTeamColor => new(1f, .4549f, .4235f, 0.6f);
    public static Color BlueTeamColor => new(.565f, .835f, 1f, 0.6f);

    public static UserProfile EmptyUser => new("0", "default", "0", "", false, [], new(0, 0, 0, 0, 0, 0, 0));

    public static Color ResultsBgColor => new(1f, .65f, 0f);

    public static Color WinningBgColor => new Color(0f, 0f, 1f);
    
    public static Color LosingBgColor => new Color(1f, 0f, 0f);
}