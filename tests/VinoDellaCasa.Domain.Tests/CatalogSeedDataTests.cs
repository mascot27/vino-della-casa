using VinoDellaCasa.Domain;
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
        Assert.Equal(153, entries.Count);
        Assert.Equal(CatalogSeedData.DemoCount + CatalogSeedData.BourgogneCount, entries.Count);
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
    public void CreateEntries_IncludesBourgogneMappedToMapHotspot()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        var bourgogne = entries
            .Where(e => WineMapRegions.Resolve(e.Appellation, e.Country) == WineMapRegion.Bourgogne)
            .ToList();

        Assert.True(bourgogne.Count >= CatalogSeedData.BourgogneCount);
        Assert.Contains(entries, e => e.Name.Contains("Chablis", StringComparison.OrdinalIgnoreCase));
        Assert.All(bourgogne, e =>
        {
            Assert.StartsWith("img/catalog/estate-", e.ImagePath, StringComparison.Ordinal);
            Assert.DoesNotContain("://", e.ImagePath, StringComparison.Ordinal);
        });

        // Stable UUID v5 for bg-001 (URL namespace + vino-della-casa/catalog/bg-001).
        var bg001 = CatalogSeedIds.FromKey("bg-001");
        Assert.Equal(Guid.Parse("b0633efa-02a1-5b8d-b7bb-537b4d686ec5"), bg001);
        Assert.Contains(entries, e => e.Id == bg001);
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
            Assert.All(e.BlendsByVintage, row =>
            {
                Assert.All(row.Components, c =>
                {
                    Assert.False(CatalogScrub.ContainsPersonalMarker(c.Grape));
                    Assert.DoesNotContain("<", c.Grape, StringComparison.Ordinal);
                });
            });
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
        });
        Assert.Contains(entries, e => !string.IsNullOrWhiteSpace(e.DrinkWindow));
        Assert.Contains(entries, e => !string.IsNullOrWhiteSpace(e.Blend));
        // Demo seed has no critic numbers; Bourgogne JSON may carry factual scores.
        Assert.All(entries.Take(CatalogSeedData.DemoCount), e =>
        {
            Assert.Null(e.ScoreRp);
            Assert.Null(e.ScoreJs);
        });
        Assert.Contains(entries.Skip(CatalogSeedData.DemoCount), e => e.ScoreRp is not null || e.ScoreJs is not null);
        Assert.Contains(entries.Skip(CatalogSeedData.DemoCount), e => e.ScoreWs is not null);
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
    public void CreateEntries_BlendsByVintage_ScrubbedDemoWhereAvailable()
    {
        var entries = CatalogSeedData.CreateEntries(2026);

        Assert.All(entries, e => Assert.NotNull(e.BlendsByVintage));

        var cheval = entries.Single(e => e.Name == "Château Cheval Blanc");
        Assert.Equal(3, cheval.BlendsByVintage.Count);
        Assert.Contains(cheval.BlendsByVintage, b => b.Vintage == 2022 && b.Components.Count == 3);

        var leoville = entries.Single(e => e.Name == "Château Léoville Barton");
        Assert.Equal(2, leoville.BlendsByVintage.Count);

        // Most fiches still omit blendsByVintage → plain Blend fallback.
        Assert.Contains(entries, e => e.BlendsByVintage.Count == 0 && !string.IsNullOrWhiteSpace(e.Blend));
        Assert.True(entries.Count(e => e.BlendsByVintage.Count > 0) >= 5);
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
