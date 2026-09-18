using System.Text.Json;
using VinoDellaCasa.Domain.Maturity;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Mcp.Handlers;
using VinoDellaCasa.Mcp.Models;

namespace VinoDellaCasa.Mcp.Tests;

public class ToolHandlersParityTests
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    [Fact]
    public void ListSeedBottles_2026_MatchesDemoSample()
    {
        var json = ToolHandlers.ListSeedBottles(new ListSeedBottlesInput { AsOfYear = 2026 });
        var root = Parse(json);
        var domain = DemoSample50SeedData.CreateSeeds(2026);
        var ready = domain.Count(b => b.ReadyToDrink);

        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal(53, root.GetProperty("count").GetInt32());
        Assert.Equal(ready, root.GetProperty("readyCount").GetInt32());

        var bottles = root.GetProperty("bottles").EnumerateArray().ToList();
        Assert.Equal(53, bottles.Count);
        Assert.Equal(ready, bottles.Count(b => b.GetProperty("readyToDrink").GetBoolean()));
    }

    [Fact]
    public void EvaluateMaturity_MatchesDomainWindowAndReady()
    {
        var json = ToolHandlers.EvaluateMaturity(new EvaluateMaturityInput
        {
            Vintage = 2018,
            Color = "Red",
            Region = "Pessac-Léognan",
            Country = "France",
            Name = "Domaine de la Solitude",
            MaturityStyle = "PessacRougeAccessible",
            AsOfYear = 2026
        });
        var root = Parse(json);

        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.True(root.GetProperty("readyToDrink").GetBoolean());
        Assert.Equal("Prêt", root.GetProperty("labelFr").GetString());
        Assert.Equal("PessacRougeAccessible", root.GetProperty("maturityStyle").GetString());
        Assert.Equal(6, root.GetProperty("window").GetProperty("peakOpenAge").GetInt32());
        Assert.Equal(10, root.GetProperty("window").GetProperty("peakCloseAge").GetInt32());
        Assert.Equal(8, root.GetProperty("ageYears").GetInt32());

        Assert.True(MaturityRules.IsReadyToDrink(2018, MaturityStyle.PessacRougeAccessible, 2026));
    }

    [Fact]
    public void EvaluateMaturity_NullVintage_NotReady()
    {
        var json = ToolHandlers.EvaluateMaturity(new EvaluateMaturityInput
        {
            Vintage = null,
            Color = "Red",
            AsOfYear = 2026
        });
        var root = Parse(json);
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.False(root.GetProperty("readyToDrink").GetBoolean());
        Assert.Equal("Sans millésime", root.GetProperty("labelFr").GetString());
    }

    [Fact]
    public void RankDrinkTonight_Seeds_OrdersOldestReadyFirst()
    {
        var seeds = M1SeedData.CreateM1Seeds(2026);
        var bottles = seeds.Select(b => new BottleInput
        {
            Id = b.Id.ToString(),
            Name = b.Name,
            Vintage = b.Vintage,
            Color = b.Color.ToString(),
            Region = b.Region,
            Quantity = b.Quantity,
            Status = b.Status.ToString(),
            ReadyToDrink = b.ReadyToDrink,
            Notes = b.Notes
        }).ToList();

        var json = ToolHandlers.RankDrinkTonight(new RankDrinkTonightInput
        {
            Bottles = bottles,
            TopN = 5,
            AsOfYear = 2026,
            RecalculateReady = true
        });
        var root = Parse(json);
        Assert.True(root.GetProperty("ok").GetBoolean());
        var items = root.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(5, items.Count);
        Assert.Equal("Château La Fleur de Gay", items[0].GetProperty("name").GetString());
        Assert.Equal("Château Troplong Mondot", items[1].GetProperty("name").GetString());
        Assert.Equal(1, items[0].GetProperty("rank").GetInt32());
        Assert.True(items.All(i => i.GetProperty("readyLabelFr").GetString() == "Prêt"));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("emptyReasonFr").ValueKind);
    }

    [Fact]
    public void RankDrinkTonight_EmptyCellar_ReturnsEmptyReason()
    {
        var json = ToolHandlers.RankDrinkTonight(new RankDrinkTonightInput
        {
            Bottles = [],
            AsOfYear = 2026
        });
        var root = Parse(json);
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Empty(root.GetProperty("items").EnumerateArray());
        Assert.Contains("Cave vide", root.GetProperty("emptyReasonFr").GetString());
    }

    [Fact]
    public void SuggestVarietalRegion_Nebbiolo()
    {
        var json = ToolHandlers.SuggestVarietalRegion(new SuggestVarietalRegionInput
        {
            Field = "varietal",
            Query = "neb",
            Limit = 8
        });
        var root = Parse(json);
        Assert.True(root.GetProperty("ok").GetBoolean());
        var suggestions = root.GetProperty("suggestions").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Contains("Nebbiolo", suggestions);
    }

    [Fact]
    public void GetColorLabels_HasFiveColorsAndBadges()
    {
        var json = ToolHandlers.GetColorLabels();
        var root = Parse(json);
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal(5, root.GetProperty("colors").GetArrayLength());
        Assert.Equal(2, root.GetProperty("maturityBadges").GetArrayLength());
        Assert.Equal("#6B1E2A", root.GetProperty("colors")[0].GetProperty("hex").GetString());
    }

    [Fact]
    public void RankDrinkTonight_RejectsTooManyBottles()
    {
        var bottles = Enumerable.Range(0, ToolLimits.MaxBottles + 1)
            .Select(i => new BottleInput { Name = $"B{i}", Color = "Red", Quantity = 1, Status = "InStock" })
            .ToList();
        var json = ToolHandlers.RankDrinkTonight(new RankDrinkTonightInput { Bottles = bottles, AsOfYear = 2026 });
        var root = Parse(json);
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal("too_many_bottles", root.GetProperty("error").GetString());
    }

    [Fact]
    public void EvaluateMaturity_RejectsInvalidColor()
    {
        var json = ToolHandlers.EvaluateMaturity(new EvaluateMaturityInput
        {
            Color = "Purple",
            AsOfYear = 2026
        });
        var root = Parse(json);
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal("invalid_color", root.GetProperty("error").GetString());
    }

    [Fact]
    public void SuggestVarietalRegion_RejectsInvalidField()
    {
        var json = ToolHandlers.SuggestVarietalRegion(new SuggestVarietalRegionInput { Field = "price" });
        var root = Parse(json);
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal("invalid_field", root.GetProperty("error").GetString());
    }
}
