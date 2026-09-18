using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Services;

namespace VinoDellaCasa.Domain.Tests;

public class DrinkTonightTests
{
    [Fact]
    public void Shortlist_ExcludesFinishedAndZeroQuantity()
    {
        var bottles = new List<Bottle>
        {
            new() { Name = "A", Color = Color.Red, Vintage = 2015, Quantity = 1, ReadyToDrink = true, Status = BottleStatus.InStock },
            new() { Name = "B", Color = Color.Red, Vintage = 2010, Quantity = 0, ReadyToDrink = true, Status = BottleStatus.InStock },
            new() { Name = "C", Color = Color.Red, Vintage = 2012, Quantity = 1, ReadyToDrink = true, Status = BottleStatus.Finished },
        };

        var result = DrinkTonight.Shortlist(bottles, topN: 5, asOfYear: 2026, recalculateReady: false);
        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }
}
