using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Filtering;

namespace VinoDellaCasa.Domain.Tests;

public class CaveBottleFilterTests
{
    private static Bottle B(string name, string? producer = null, string? region = null,
        Color color = Color.Red, bool ready = true, string? bin = "A1") => new()
        {
            Name = name,
            Producer = producer,
            Region = region,
            Color = color,
            ReadyToDrink = ready,
            Bin = bin,
            Quantity = 1,
            Status = BottleStatus.InStock
        };

    [Fact]
    public void Text_MatchesNameProducerRegion()
    {
        var all = new[]
        {
            B("Château X", "Prod A", "Pomerol"),
            B("Autre", "Domaine Y", "Margaux")
        };

        Assert.Single(CaveBottleFilter.Apply(all, text: "pomerol"));
        Assert.Single(CaveBottleFilter.Apply(all, text: "domaine"));
        Assert.Single(CaveBottleFilter.Apply(all, text: "château"));
    }

    [Fact]
    public void Color_Ready_Bin_Compose()
    {
        var all = new[]
        {
            B("R1", color: Color.Red, ready: true, bin: "A1"),
            B("W1", color: Color.White, ready: false, bin: "B2"),
            B("R2", color: Color.Red, ready: false, bin: "A1")
        };

        var q = CaveBottleFilter.Apply(all, color: Color.Red, readyToDrink: true, bin: "A").ToList();
        Assert.Single(q);
        Assert.Equal("R1", q[0].Name);
    }
}
