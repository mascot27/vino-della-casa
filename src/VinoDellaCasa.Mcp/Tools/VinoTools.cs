using System.ComponentModel;
using ModelContextProtocol.Server;
using VinoDellaCasa.Mcp.Handlers;
using VinoDellaCasa.Mcp.Models;

namespace VinoDellaCasa.Mcp.Tools;

/// <summary>
/// MCP tool surface (stdio only). Read-only domain compute — no network, no FS writes, no secrets.
/// </summary>
[McpServerToolType]
public sealed class VinoTools
{
    [McpServerTool(Name = "evaluate_maturity"), Description(
        "Compute ReadyToDrink and maturity style for one bottle (deterministic MaturityRules).")]
    public string EvaluateMaturity(
        [Description("Wine color: Red|White|Rose|Sparkling|Other")] string color,
        [Description("Vintage year, or omit/null for no vintage")] int? vintage = null,
        [Description("Region label")] string? region = null,
        [Description("Country label")] string? country = null,
        [Description("Bottle display name")] string? name = null,
        [Description("Free notes; may contain MaturityStyle=Token")] string? notes = null,
        [Description("Optional explicit MaturityStyle enum name")] string? maturityStyle = null,
        [Description("Reference calendar year (default: Europe/Zurich current year)")] int? asOfYear = null)
    {
        return ToolHandlers.EvaluateMaturity(new EvaluateMaturityInput
        {
            Color = color,
            Vintage = vintage,
            Region = region,
            Country = country,
            Name = name,
            Notes = notes,
            MaturityStyle = maturityStyle,
            AsOfYear = asOfYear
        });
    }

    [McpServerTool(Name = "rank_drink_tonight"), Description(
        "Shortlist bottles ready to drink tonight (InStock, qty>0, ready). Sorted vintage ASC, name ASC.")]
    public string RankDrinkTonight(
        [Description("Candidate bottles (max 200)")] List<BottleInput>? bottles = null,
        [Description("Max results 1..20 (default 5)")] int? topN = null,
        [Description("Reference calendar year")] int? asOfYear = null,
        [Description("If true, recompute ReadyToDrink via MaturityRules")] bool? recalculateReady = null,
        [Description("Optional color filter: Red|White|Rose|Sparkling|Other")] string? colorFilter = null)
    {
        return ToolHandlers.RankDrinkTonight(new RankDrinkTonightInput
        {
            Bottles = bottles,
            TopN = topN,
            AsOfYear = asOfYear,
            RecalculateReady = recalculateReady,
            ColorFilter = colorFilter
        });
    }

    [McpServerTool(Name = "list_seed_bottles"), Description(
        "Return anonymous Bordeaux demo cellar seeds (50). Default asOfYear=2026.")]
    public string ListSeedBottles(
        [Description("Reference year for ReadyToDrink (default 2026)")] int? asOfYear = null,
        [Description("If true, only ready bottles")] bool? includeReadyOnly = null)
    {
        return ToolHandlers.ListSeedBottles(new ListSeedBottlesInput
        {
            AsOfYear = asOfYear,
            IncludeReadyOnly = includeReadyOnly
        });
    }

    [McpServerTool(Name = "suggest_varietal_region"), Description(
        "Autocomplete suggestions for varietal, region, or country (generic lists).")]
    public string SuggestVarietalRegion(
        [Description("Field: varietal|region|country")] string field,
        [Description("Case-insensitive substring; empty = top of list")] string? query = null,
        [Description("Max suggestions 1..32 (default 8)")] int? limit = null)
    {
        return ToolHandlers.SuggestVarietalRegion(new SuggestVarietalRegionInput
        {
            Field = field,
            Query = query,
            Limit = limit
        });
    }

    [McpServerTool(Name = "get_color_labels"), Description(
        "French color labels, hex tokens, and maturity badges.")]
    public string GetColorLabels()
        => ToolHandlers.GetColorLabels();
}
