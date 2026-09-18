using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Tests;

public class BottleTests
{
    [Fact]
    public void NewBottle_DefaultsStatusToInStock_AndGeneratesId()
    {
        var bottle = new Bottle
        {
            Name = "Barolo Riserva",
            Color = Color.Red,
            Quantity = 2
        };

        Assert.NotEqual(Guid.Empty, bottle.Id);
        Assert.Equal(BottleStatus.InStock, bottle.Status);
        Assert.Equal("Barolo Riserva", bottle.Name);
        Assert.Equal(2, bottle.Quantity);
    }

    [Fact]
    public void Bottle_CanRepresentFinishedStatus()
    {
        var bottle = new Bottle
        {
            Name = "Champagne Brut",
            Color = Color.Sparkling,
            Quantity = 0,
            Status = BottleStatus.Finished
        };

        Assert.Equal(BottleStatus.Finished, bottle.Status);
        Assert.Equal(0, bottle.Quantity);
    }
}
