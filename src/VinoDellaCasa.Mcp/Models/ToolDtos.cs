using System.ComponentModel;
using System.Text.Json.Serialization;

namespace VinoDellaCasa.Mcp.Models;

public sealed class EvaluateMaturityInput
{
    public int? Vintage { get; set; }
    public string? Color { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public string? MaturityStyle { get; set; }
    public int? AsOfYear { get; set; }
}

public sealed class RankDrinkTonightInput
{
    public List<BottleInput>? Bottles { get; set; }
    public int? TopN { get; set; }
    public int? AsOfYear { get; set; }
    public bool? RecalculateReady { get; set; }
    public string? ColorFilter { get; set; }
}

public sealed class BottleInput
{
    [Description("Stable bottle id")]
    public string? Id { get; set; }

    [Description("Display name")]
    public string? Name { get; set; }

    [Description("Vintage year")]
    public int? Vintage { get; set; }

    [Description("Red|White|Rose|Sparkling|Other")]
    public string? Color { get; set; }

    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }

    [Description("Quantity in stock (0..10000)")]
    public int? Quantity { get; set; }

    [Description("InStock|Drinking|Finished")]
    public string? Status { get; set; }

    [Description("Precomputed ready flag if recalculateReady=false")]
    public bool? ReadyToDrink { get; set; }
}

public sealed class ListSeedBottlesInput
{
    public int? AsOfYear { get; set; }
    public bool? IncludeReadyOnly { get; set; }
}

public sealed class SuggestVarietalRegionInput
{
    public string? Field { get; set; }
    public string? Query { get; set; }
    public int? Limit { get; set; }
}

public sealed class ErrorResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("error")]
    public required string Error { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
