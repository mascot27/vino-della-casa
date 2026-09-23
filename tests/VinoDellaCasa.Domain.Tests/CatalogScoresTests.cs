using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Seed;

namespace VinoDellaCasa.Domain.Tests;

public class CatalogScoresTests
{
    [Fact]
    public void ForEntry_BuildsStructuredChips_InStableOrder()
    {
        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Appellation = "Pauillac",
            ImagePath = "img/catalog/estate-chateau.jpg",
            Style = "structure",
            Pourquoi = "profil classique",
            Color = Color.Red,
            ScoreRp = 96,
            ScoreJs = 94,
            ScoreWs = 93,
            ScoreHachette = "**",
            Stars = 3
        };

        var chips = CatalogScores.ForEntry(entry);
        Assert.Equal(5, chips.Count);
        Assert.Equal("RP 96", chips[0].Label);
        Assert.Equal("JS 94", chips[1].Label);
        Assert.Equal("WS 93", chips[2].Label);
        Assert.Equal("Hachette **", chips[3].Label);
        Assert.Equal("★ 3", chips[4].Label);
    }

    [Fact]
    public void ForEntry_EmptyWhenAbsent_GracefulFallback()
    {
        Assert.Empty(CatalogScores.ForEntry(null));
        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(),
            Name = "Sans scores",
            Appellation = "Bourgogne",
            ImagePath = "img/catalog/estate-vineyard.jpg",
            Style = "fruité",
            Pourquoi = "profil souple",
            Color = Color.White
        };
        Assert.Empty(CatalogScores.ForEntry(entry));
    }

    [Fact]
    public void ForEntry_SanitizesHachetteMark()
    {
        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Appellation = "Barsac",
            ImagePath = "img/catalog/estate-chateau.jpg",
            Style = "liquoreux",
            Pourquoi = "profil liquoreux",
            Color = Color.White,
            ScoreHachette = " <b>**</b> "
        };
        var chips = CatalogScores.ForEntry(entry);
        Assert.Single(chips);
        Assert.Equal("Hachette **", chips[0].Label);
    }
}
