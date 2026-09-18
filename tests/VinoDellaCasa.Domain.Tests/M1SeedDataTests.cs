using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Domain.Services;

namespace VinoDellaCasa.Domain.Tests;

public class M1SeedDataTests
{
    [Fact]
    public void CreateM1Seeds_2026_HasTenBottles_WithSevenReadyAndThreeTooYoung()
    {
        var seeds = M1SeedData.CreateM1Seeds(2026);

        Assert.Equal(10, seeds.Count);
        Assert.Equal(8, seeds.Count(b => b.Color == Color.Red));
        Assert.Equal(2, seeds.Count(b => b.Color == Color.White));
        Assert.Equal(8, seeds.Count(b => b.Country == "France"));
        Assert.Equal(2, seeds.Count(b => b.Country == "Suisse"));
        Assert.All(seeds, b => Assert.Equal(BottleStatus.InStock, b.Status));

        var readyNames = seeds.Where(b => b.ReadyToDrink).Select(b => b.Name).OrderBy(n => n).ToList();
        var notReadyNames = seeds.Where(b => !b.ReadyToDrink).Select(b => b.Name).OrderBy(n => n).ToList();

        Assert.Equal(7, readyNames.Count);
        Assert.Equal(3, notReadyNames.Count);

        Assert.Contains("Domaine de la Solitude", readyNames);
        Assert.Contains("Château La Fleur de Gay", readyNames);
        Assert.Contains("Pagode de Cos", readyNames);
        Assert.Contains("L’Esprit de Chevalier", readyNames);
        Assert.Contains("Château Troplong Mondot", readyNames);
        Assert.Contains("Clos des Abbayes", readyNames);
        Assert.Contains("Petite Arvine", readyNames);

        Assert.Contains("Château Larcis Ducasse", notReadyNames);
        Assert.Contains("Château Pape Clément", notReadyNames);
        Assert.Contains("Domaine de Chevalier", notReadyNames);

        var nullPrice = seeds.Where(b => b.PriceChf is null).Select(b => b.Name).OrderBy(n => n).ToList();
        Assert.Equal(
            new[] { "Clos des Abbayes", "L’Esprit de Chevalier", "Petite Arvine" }.OrderBy(n => n),
            nullPrice);
    }

    [Fact]
    public void DrinkTonight_OrdersReadyFirst_ThenOldestVintage_ThenName()
    {
        var seeds = M1SeedData.CreateM1Seeds(2026);
        var shortlist = DrinkTonight.Shortlist(seeds, topN: 5, asOfYear: 2026, recalculateReady: false);

        Assert.Equal(5, shortlist.Count);
        Assert.All(shortlist, b => Assert.True(b.ReadyToDrink));
        Assert.Equal("Château La Fleur de Gay", shortlist[0].Name);
        Assert.Equal("Château Troplong Mondot", shortlist[1].Name);
    }
}
