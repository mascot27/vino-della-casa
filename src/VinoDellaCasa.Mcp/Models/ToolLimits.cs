namespace VinoDellaCasa.Mcp.Models;

/// <summary>Hard caps for MCP tool inputs (strict schemas / DoS guard).</summary>
public static class ToolLimits
{
    public const int MaxBottles = 200;
    public const int MaxTopN = 20;
    public const int DefaultTopN = 5;
    public const int MaxSuggestionLimit = 32;
    public const int DefaultSuggestionLimit = 8;
    public const int MaxStringLength = 200;
    public const int MaxNotesLength = 2000;
    public const int MinVintage = 1800;
    public const int MaxVintage = 2100;
    public const int MinAsOfYear = 1900;
    public const int MaxAsOfYear = 2200;
}
