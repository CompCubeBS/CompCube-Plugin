using CompCube_Models.Models.ClientData;

namespace CompCube.Extensions;

public static class DivisionExtensions
{
    public static string GetFormattedDivision(this DivisionInfo divisionInfo) => $"{divisionInfo.Division} {divisionInfo.SubDivision}".FormatWithHtmlColor(divisionInfo.Color);
}