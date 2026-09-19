using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Seed;

namespace VinoDellaCasa.Domain.Tests;

public class CatalogSeedDataTests
{
    [Fact]
    public void CreateEntries_HasExpectedAnonymousCount_AndRequiredFields()
    {
        var entries = CatalogSeedData.CreateEntries(2026);

        Assert.Equal(CatalogSeedData.ExpectedCount, entries.Count);
        Assert.Equal(53, entries.Count);
        Assert.Equal(entries.Count, entries.Select(e => e.Id).Distinct().Count());

        Assert.All(entries, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name));
            Assert.False(string.IsNullOrWhiteSpace(e.Appellation));
            Assert.False(string.IsNullOrWhiteSpace(e.ImagePath));
            Assert.StartsWith("img/catalog/", e.ImagePath, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(e.Style));
            Assert.False(string.IsNullOrWhiteSpace(e.Pourquoi));
            Assert.DoesNotContain("http://", e.ImagePath, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("https://", e.ImagePath, StringComparison.OrdinalIgnoreCase);
        });

        Assert.Contains(entries, e => e.Name == "Domaine de la Solitude");
        Assert.Contains(entries, e => e.Color == Color.Red);
        Assert.Contains(entries, e => e.Color == Color.White);
        Assert.Contains(entries, e => e.Color == Color.Rose);
        Assert.Contains(entries, e => e.Color == Color.Sparkling);
    }

    [Fact]
    public void CreateEntries_ScrubsPersonalMarkers()
    {
        var entries = CatalogSeedData.CreateEntries(2026);

        Assert.All(entries, e =>
        {
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Name));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Producer));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Style));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Pourquoi));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Blend));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Classification));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.Pairing));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.ScoreHachette));
            Assert.False(CatalogScrub.ContainsPersonalMarker(e.PriceRangeChf));
            Assert.Null(CatalogScrub.FindPersonalLeak(e.Pourquoi));
            Assert.StartsWith("img/catalog/", e.ImagePath, StringComparison.Ordinal);
            Assert.DoesNotContain("<", e.Pourquoi, StringComparison.Ordinal);
            Assert.DoesNotContain("<", e.Style, StringComparison.Ordinal);
            Assert.DoesNotContain("<", e.Pairing ?? string.Empty, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void FindById_ReturnsMatchingEntry_AndUnknownIsNull()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        var first = entries[0];

        var found = CatalogSeedData.FindById(first.Id, 2026);
        Assert.NotNull(found);
        Assert.Equal(first.Id, found.Id);
        Assert.Equal(first.Name, found.Name);

        Assert.Null(CatalogSeedData.FindById(Guid.Empty, 2026));
    }

    [Fact]
    public void CreateEntries_HasPairingAndLocalImagesOnly()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        Assert.All(entries, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Pairing));
            Assert.DoesNotContain("http://", e.ImagePath, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("https://", e.ImagePath, StringComparison.OrdinalIgnoreCase);
            // Scores / price optional — demo seed has no critic numbers yet.
            Assert.Null(e.ScoreRp);
            Assert.Null(e.ScoreJs);
        });
        Assert.Contains(entries, e => !string.IsNullOrWhiteSpace(e.DrinkWindow));
        Assert.Contains(entries, e => !string.IsNullOrWhiteSpace(e.Blend));
    }

    [Fact]
    public void CatalogScrub_RemovesPersonalTags()
    {
        var scrubbed = CatalogScrub.ScrubTags(
        [
            "pessac-style",
            "corentin-core",
            "wedding-stock",
            "accessible",
            "gravel"
        ]);

        Assert.Equal(3, scrubbed.Count);
        Assert.Contains("pessac-style", scrubbed);
        Assert.Contains("accessible", scrubbed);
        Assert.Contains("gravel", scrubbed);
        Assert.DoesNotContain("corentin-core", scrubbed);
        Assert.DoesNotContain("wedding-stock", scrubbed);

        Assert.True(CatalogScrub.ContainsPersonalMarker("notesInternal"));
        Assert.True(CatalogScrub.ContainsPersonalMarker("corentin-core"));
        Assert.True(CatalogScrub.ContainsPersonalMarker("wedding-stock"));
        Assert.False(CatalogScrub.ContainsPersonalMarker("pessac-style"));
    }

    [Fact]
    public void DomaineDeLaSolitude_MatchesMockStylePourquoi()
    {
        var solitude = CatalogSeedData.CreateEntries(2026)
            .Single(e => e.Name == "Domaine de la Solitude");

        Assert.Equal("fruité · structure souple", solitude.Style);
        Assert.False(string.IsNullOrWhiteSpace(solitude.Pourquoi));
        Assert.False(CatalogScrub.ContainsPersonalMarker(solitude.Pourquoi));
        Assert.Equal(Color.Red, solitude.Color);
        Assert.Equal(2021, solitude.Vintage);
        Assert.Equal("Pessac-Léognan", solitude.Appellation);
        // 2021 as of 2026 → age 5; Pessac accessible window opens at 6 → not yet ready.
        Assert.False(solitude.ReadyToDrink);
        Assert.Contains("attendre", solitude.Pourquoi, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToBottle_PreservesCatalogId()
    {
        var entry = CatalogSeedData.CreateEntries(2026)[0];
        var bottle = CatalogSeedData.ToBottle(entry, quantity: 2, asOfYear: 2026);

        Assert.Equal(entry.Id, bottle.Id);
        Assert.Equal(2, bottle.Quantity);
        Assert.Equal(entry.Name, bottle.Name);
        Assert.Equal(entry.Appellation, bottle.Region);
        Assert.Null(bottle.Notes);
        Assert.Null(bottle.PriceChf);
    }
}
