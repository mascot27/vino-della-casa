using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Domain.Services;

namespace VinoDellaCasa.Domain.Tests;

public class DemoSample50SeedDataTests
{
    [Fact]
    public void CreateSeeds_2026_HasFiftyAnonymousBordeauxBottles()
    {
        var seeds = DemoSample50SeedData.CreateSeeds(2026);

        Assert.Equal(50, seeds.Count);
        Assert.All(seeds, b => Assert.Equal(BottleStatus.InStock, b.Status));
        Assert.All(seeds, b => Assert.Equal("France", b.Country));
        Assert.All(seeds, b => Assert.Null(b.Notes));
        Assert.All(seeds, b => Assert.Null(b.PriceChf));
        Assert.Equal(seeds.Count, seeds.Select(b => b.Id).Distinct().Count());

        Assert.True(seeds.Count(b => b.ReadyToDrink) >= 20);
        Assert.True(seeds.Count(b => !b.ReadyToDrink) >= 5);
        Assert.Contains(seeds, b => b.Color == Color.Red);
        Assert.Contains(seeds, b => b.Color == Color.White);
    }

    [Fact]
    public void DrinkTonight_FromDemo50_ReturnsReadyShortlist()
    {
        var seeds = DemoSample50SeedData.CreateSeeds(2026);
        var shortlist = DrinkTonight.Shortlist(seeds, topN: 5, asOfYear: 2026, recalculateReady: false);

        Assert.Equal(5, shortlist.Count);
        Assert.All(shortlist, b => Assert.True(b.ReadyToDrink));
    }
}
