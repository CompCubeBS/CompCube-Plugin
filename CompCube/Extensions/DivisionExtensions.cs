using CompCube.Models;

namespace CompCube.Extensions;

public static class DivisionExtensions
{
    public static string GetFormattedDivision(this DivisionInfo divisionInfo) => $"{divisionInfo.Division} {divisionInfo.SubDivision}".FormatWithHtmlColor(divisionInfo.Color);
}