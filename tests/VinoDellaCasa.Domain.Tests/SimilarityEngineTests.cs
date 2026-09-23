using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Pairing;
using VinoDellaCasa.Domain.Seed;

namespace VinoDellaCasa.Domain.Tests;

/// <summary>Fixtures from Docs/domain/SIMILARITY-RULES.md + issue #74.</summary>
public class SimilarityEngineTests
{
    private static IReadOnlyList<WinePairingProfile> CatalogProfiles(int asOfYear = 2026) =>
        CatalogSeedData.CreateEntries(asOfYear)
            .Select(PairingProfileBuilder.FromCatalog)
            .ToList();

    [Fact]
    public void Fixture01_Solitude_SuggestsOtherPessacMedium()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        var solitude = entries.First(e =>
            e.Name.Contains("Solitude", StringComparison.OrdinalIgnoreCase) && e.Color == Color.Red);
        var source = PairingProfileBuilder.FromCatalog(solitude);
        var matches = SimilarityEngine.FindSimilar(source, CatalogProfiles());

        Assert.InRange(matches.Count, 1, 5);
        Assert.All(matches, m => Assert.Equal(Color.Red, m.Profile.Color));
        Assert.Contains(matches, m =>
            ContainsAny(m.Profile.Region, "Pessac", "Graves")
            || ContainsAny(m.Why, "Bordeaux", "Pessac", "structure", "famille"));
        Assert.DoesNotContain(matches, m => m.Profile.Id == source.Id);
    }

    [Fact]
    public void Fixture02_Chablis_SuggestsOtherChablisOrMineralWhite()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        var chablis = entries.First(e =>
            e.Appellation.Contains("Chablis", StringComparison.OrdinalIgnoreCase) && e.Color == Color.White);
        var source = PairingProfileBuilder.FromCatalog(chablis);
        var matches = SimilarityEngine.FindSimilar(source, CatalogProfiles());

        Assert.InRange(matches.Count, 3, 5);
        Assert.All(matches, m => Assert.True(
            m.Profile.Color is Color.White or Color.Sparkling,
            $"Expected white/sparkling, got {m.Profile.Color}"));
        Assert.Contains(matches, m =>
            ContainsAny(m.Profile.Region, "Chablis")
            || ContainsAny(m.Why, "Chablis", "minéral", "mineral")
            || m.Profile.StyleTags.Any(t =>
                t.Contains("chablis", StringComparison.OrdinalIgnoreCase)
                || t.Contains("mineral", StringComparison.OrdinalIgnoreCase)));
        Assert.True(matches[0].Score >= 60);
    }

    [Fact]
    public void Fixture03_Pomerol_SuggestsRightBankMerlotPeers()
    {
        var entries = CatalogSeedData.CreateEntries(2026);
        var pomerol = entries.First(e =>
            e.Appellation.Contains("Pomerol", StringComparison.OrdinalIgnoreCase) && e.Color == Color.Red);
        var source = PairingProfileBuilder.FromCatalog(pomerol);
        var matches = SimilarityEngine.FindSimilar(source, CatalogProfiles());

        Assert.NotEmpty(matches);
        Assert.All(matches, m => Assert.Equal(Color.Red, m.Profile.Color));
        Assert.Contains(matches, m =>
            ContainsAny(m.Profile.Region, "Pomerol", "Saint-Émilion", "Saint-Emilion")
            || ContainsAny(m.Why, "Bordeaux", "Pomerol", "famille"));
    }

    [Fact]
    public void Fixture04_Sauternes_DoesNotMatchDryMedoc()
    {
        var sauternes = PairingProfileBuilder.InferFrom(
            Guid.NewGuid(),
            "Château d'Yquem",
            Color.White,
            "Sauternes",
            "liquoreux · miel",
            "Sémillon, Sauvignon",
            null,
            "Foie gras",
            readyToDrink: true,
            styleTags: ["liquoreux"],
            vintage: 2015);

        var medoc = PairingProfileBuilder.InferFrom(
            Guid.NewGuid(),
            "Château fictif Médoc",
            Color.Red,
            "Haut-Médoc",
            "cabernet · structuré",
            "Cabernet Sauvignon",
            null,
            null,
            readyToDrink: true,
            styleTags: ["cabernet"],
            vintage: 2016);

        var otherSauternes = PairingProfileBuilder.InferFrom(
            Guid.NewGuid(),
            "Château Suduiraut",
            Color.White,
            "Sauternes",
            "liquoreux",
            "Sémillon",
            null,
            null,
            readyToDrink: true,
            styleTags: ["liquoreux"],
            vintage: 2016);

        var matches = SimilarityEngine.FindSimilar(sauternes, [medoc, otherSauternes]);

        Assert.DoesNotContain(matches, m => m.Profile.Id == medoc.Id);
        Assert.Contains(matches, m => m.Profile.Id == otherSauternes.Id);
    }

    [Fact]
    public void Fixture05_SameColorStructureAndRegion_ExplainableWhy()
    {
        var source = new WinePairingProfile
        {
            Id = Guid.NewGuid(),
            Name = "Source",
            Color = Color.Red,
            Region = "Pessac-Léognan",
            Body = TasteBody.Medium,
            Tannin = TasteTannin.Medium,
            Acidity = TasteAcidity.Medium,
            Oak = TasteOak.Subtle,
            ReadyToDrink = true,
            Vintage = 2018
        };
        var peer = new WinePairingProfile
        {
            Id = Guid.NewGuid(),
            Name = "Peer Pessac",
            Color = Color.Red,
            Region = "Pessac-Léognan",
            Body = TasteBody.Medium,
            Tannin = TasteTannin.Medium,
            Acidity = TasteAcidity.Medium,
            Oak = TasteOak.Subtle,
            ReadyToDrink = true,
            Vintage = 2019
        };

        var matches = SimilarityEngine.FindSimilar(source, [peer]);
        Assert.Single(matches);
        Assert.True(matches[0].Score >= 80);
        Assert.False(string.IsNullOrWhiteSpace(matches[0].Why));
        Assert.Contains("Pessac", matches[0].Why, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HardFilter_ExcludesDifferentColor_ExceptSparklingWhiteSoft()
    {
        var white = new WinePairingProfile
        {
            Id = Guid.NewGuid(),
            Color = Color.White,
            Region = "Chablis",
            Body = TasteBody.Medium,
            Acidity = TasteAcidity.High,
            ReadyToDrink = true
        };
        var red = new WinePairingProfile
        {
            Id = Guid.NewGuid(),
            Color = Color.Red,
            Region = "Chablis",
            Body = TasteBody.Medium,
            ReadyToDrink = true
        };
        var sparkling = new WinePairingProfile
        {
            Id = Guid.NewGuid(),
            Color = Color.Sparkling,
            Region = "Champagne",
            Body = TasteBody.Light,
            Acidity = TasteAcidity.High,
            ReadyToDrink = true
        };

        var vsRed = SimilarityEngine.FindSimilar(white, [red]);
        Assert.Empty(vsRed);

        var vsSparkling = SimilarityEngine.FindSimilar(white, [sparkling]);
        Assert.Single(vsSparkling);
        Assert.True(vsSparkling[0].Score > 0);
    }

    [Fact]
    public void CapsAtTopN_AndExcludesSelf()
    {
        var profiles = CatalogProfiles();
        var source = profiles.First(p => p.Color == Color.Red);
        var matches = SimilarityEngine.FindSimilar(source, profiles, topN: 3);
        Assert.True(matches.Count <= 3);
        Assert.DoesNotContain(matches, m => m.Profile.Id == source.Id);
    }

    private static bool ContainsAny(string? hay, params string[] needles)
    {
        if (string.IsNullOrEmpty(hay))
        {
            return false;
        }

        return needles.Any(n => hay.Contains(n, StringComparison.OrdinalIgnoreCase));
    }
}
