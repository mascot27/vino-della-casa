using VinoDellaCasa.Domain;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Tests;

public class WineMapRegionsTests
{
    [Theory]
    [InlineData("Pomerol", "France", WineMapRegion.Bordeaux)]
    [InlineData("Saint-Émilion Grand Cru", "France", WineMapRegion.Bordeaux)]
    [InlineData("Pessac-Léognan", "France", WineMapRegion.Bordeaux)]
    [InlineData("Haut-Médoc", "France", WineMapRegion.Bordeaux)]
    [InlineData("Barsac", "France", WineMapRegion.Bordeaux)]
    [InlineData("Sauternes", "France", WineMapRegion.Bordeaux)]
    [InlineData("Bordeaux", "France", WineMapRegion.Bordeaux)]
    [InlineData("Bourgogne", "France", WineMapRegion.Bourgogne)]
    [InlineData("Côte de Nuits", "France", WineMapRegion.Bourgogne)]
    [InlineData("Burgundy", "France", WineMapRegion.Bourgogne)]
    [InlineData("Champagne", "France", WineMapRegion.Champagne)]
    [InlineData("Sancerre", "France", WineMapRegion.Loire)]
    [InlineData("Alsace", "France", WineMapRegion.Alsace)]
    [InlineData("Côte-Rôtie", "France", WineMapRegion.Rhone)]
    [InlineData("Châteauneuf-du-Pape", "France", WineMapRegion.Rhone)]
    [InlineData("Côtes de Provence", "France", WineMapRegion.Provence)]
    [InlineData("Bandol", "France", WineMapRegion.Provence)]
    [InlineData("Madiran", "France", WineMapRegion.SudOuest)]
    [InlineData("Cahors", "France", WineMapRegion.SudOuest)]
    [InlineData("Valais", "Suisse", WineMapRegion.Suisse)]
    [InlineData("Lavaux", "Suisse", WineMapRegion.Suisse)]
    [InlineData(null, "Suisse", WineMapRegion.Suisse)]
    [InlineData("Toscane", "Italie", WineMapRegion.Other)]
    public void Resolve_MatchesMappingDoc(string? region, string? country, WineMapRegion expected)
    {
        Assert.Equal(expected, WineMapRegions.Resolve(region, country));
    }

    [Fact]
    public void Resolve_Bottle_UsesRegionAndCountry()
    {
        var bottle = new Bottle
        {
            Name = "Petite Arvine",
            Region = "Valais",
            Country = "Suisse",
            Color = Color.White,
            Quantity = 1
        };
        Assert.Equal(WineMapRegion.Suisse, WineMapRegions.Resolve(bottle));
    }

    [Fact]
    public void Id_And_LabelFr_AreStable()
    {
        Assert.Equal("bordeaux", WineMapRegions.Id(WineMapRegion.Bordeaux));
        Assert.Equal("sud-ouest", WineMapRegions.Id(WineMapRegion.SudOuest));
        Assert.Equal("Rhône", WineMapRegions.LabelFr(WineMapRegion.Rhone));
        Assert.Equal("Sud-Ouest", WineMapRegions.LabelFr(WineMapRegion.SudOuest));
    }
}
