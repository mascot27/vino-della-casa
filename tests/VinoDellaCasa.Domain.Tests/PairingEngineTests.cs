using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Pairing;
using VinoDellaCasa.Domain.Seed;

namespace VinoDellaCasa.Domain.Tests;

/// <summary>10 DoD fixtures from Docs/domain/PAIRING-RULES.md.</summary>
public class PairingEngineTests
{
    [Fact]
    public void Fixture01_SolitudeRouge_SuggestsGrilladesOrAgneauOrVolaille()
    {
        var entry = CatalogSeedData.CreateEntries(2026)
            .First(e => e.Name.Contains("Solitude", StringComparison.OrdinalIgnoreCase)
                        && e.Color == Color.Red);
        var profile = PairingProfileBuilder.FromCatalog(entry);
        var dishes = PairingEngine.SuggestDishes(profile);

        Assert.NotEmpty(dishes);
        Assert.Contains(dishes, d =>
            ContainsAny(d.Dish, "agneau", "bœuf grillé", "grillades", "volaille", "champignons"));
    }

    [Fact]
    public void Fixture02_Chablis_SuggestsHuitresOrFruitsDeMer()
    {
        var entry = CatalogSeedData.CreateEntries(2026)
            .First(e => e.Appellation.Contains("Chablis", StringComparison.OrdinalIgnoreCase)
                        && e.Color == Color.White);
        var dishes = PairingEngine.SuggestDishes(PairingProfileBuilder.FromCatalog(entry));

        Assert.Contains(dishes, d => ContainsAny(d.Dish, "huîtres", "fruits de mer"));
        Assert.True(dishes[0].Score >= 80);
    }

    [Fact]
    public void Fixture03_FullFirmRed_SuggestsGibierOrBoeufMijote()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.Red,
            Body = TasteBody.Full,
            Tannin = TasteTannin.Firm,
            Acidity = TasteAcidity.High,
            Oak = TasteOak.Marked,
            ReadyToDrink = true,
            Region = "Pommard"
        };
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "gibier", "bœuf mijoté", "côte de bœuf"));
    }

    [Fact]
    public void Fixture04_Rose_SuggestsAperitifOrSalades()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.Rose,
            Body = TasteBody.Light,
            ReadyToDrink = true,
            Region = "Provence"
        };
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "apéritif", "salades", "grillades"));
    }

    [Fact]
    public void Fixture05_Sparkling_SuggestsAperitif()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.Sparkling,
            Body = TasteBody.Light,
            Acidity = TasteAcidity.High,
            ReadyToDrink = true
        };
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Equal("apéritif", dishes[0].Dish);
        Assert.True(dishes[0].Score >= 80);
    }

    [Fact]
    public void Fixture06_Sauternes_SuggestsFoieGras()
    {
        var profile = PairingProfileBuilder.InferFrom(
            Guid.NewGuid(),
            "Château d'Yquem",
            Color.White,
            "Sauternes",
            "liquoreux · miel",
            "Sémillon, Sauvignon",
            null,
            "Foie gras · desserts",
            readyToDrink: true,
            styleTags: ["liquoreux"]);
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "foie gras", "bleu"));
    }

    [Fact]
    public void Fixture07_Pomerol_SuggestsAgneauOrCanard()
    {
        var entry = CatalogSeedData.CreateEntries(2026)
            .First(e => e.Appellation.Contains("Pomerol", StringComparison.OrdinalIgnoreCase)
                        && e.Color == Color.Red);
        var dishes = PairingEngine.SuggestDishes(PairingProfileBuilder.FromCatalog(entry));
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "agneau", "canard"));
    }

    [Fact]
    public void Fixture08_MedocFirm_SuggestsBoeufGrilleOrAgneau()
    {
        var entry = CatalogSeedData.CreateEntries(2026)
            .FirstOrDefault(e => e.Color == Color.Red && (
                e.Appellation.Contains("Pauillac", StringComparison.OrdinalIgnoreCase)
                || e.Appellation.Contains("Médoc", StringComparison.OrdinalIgnoreCase)
                || e.Appellation.Contains("Saint-Estèphe", StringComparison.OrdinalIgnoreCase)
                || e.Appellation.Contains("Saint-Julien", StringComparison.OrdinalIgnoreCase)
                || e.Appellation.Contains("Margaux", StringComparison.OrdinalIgnoreCase)));

        Assert.NotNull(entry);
        var dishes = PairingEngine.SuggestDishes(PairingProfileBuilder.FromCatalog(entry!));
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "bœuf grillé", "agneau"));
    }

    [Fact]
    public void Fixture09_BeaujolaisLight_SuggestsCharcuterie()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.Red,
            Body = TasteBody.Light,
            Tannin = TasteTannin.Soft,
            Acidity = TasteAcidity.High,
            Oak = TasteOak.None,
            Region = "Beaujolais",
            StyleTags = ["gamay", "easy"],
            ReadyToDrink = true
        };
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "charcuterie", "volaille"));
    }

    [Fact]
    public void Fixture10_MeursaultLike_SuggestsHomard()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.White,
            Body = TasteBody.Full,
            Tannin = TasteTannin.Soft,
            Acidity = TasteAcidity.Medium,
            Oak = TasteOak.Marked,
            Region = "Meursault",
            ReadyToDrink = true
        };
        var dishes = PairingEngine.SuggestDishes(profile);
        Assert.Contains(dishes, d => ContainsAny(d.Dish, "homard", "poularde", "ris de veau"));
    }

    [Fact]
    public void DishToBottles_Huitres_RanksChablisHigh()
    {
        var catalog = CatalogSeedData.CreateEntries(2026)
            .Where(e => e.Color == Color.White)
            .Take(40)
            .Select(PairingProfileBuilder.FromCatalog)
            .ToList();

        var matches = PairingEngine.SuggestBottles("huîtres", catalog, topN: 5);
        Assert.NotEmpty(matches);
        Assert.Contains(matches, m =>
            (m.Profile.Region ?? string.Empty).Contains("Chablis", StringComparison.OrdinalIgnoreCase)
            || (m.Profile.Name ?? string.Empty).Contains("Chablis", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SuggestDishes_TopN_AndStableWhy()
    {
        var profile = new WinePairingProfile
        {
            Color = Color.Red,
            Body = TasteBody.Medium,
            Tannin = TasteTannin.Medium,
            ReadyToDrink = true
        };
        var dishes = PairingEngine.SuggestDishes(profile, topN: 3);
        Assert.Equal(3, dishes.Count);
        Assert.All(dishes, d =>
        {
            Assert.InRange(d.Score, 1, 100);
            Assert.False(string.IsNullOrWhiteSpace(d.Why));
        });
    }

    [Fact]
    public void SuggestDishes_ThrowsOnBadTopN()
    {
        var profile = new WinePairingProfile { Color = Color.Red };
        Assert.Throws<ArgumentOutOfRangeException>(() => PairingEngine.SuggestDishes(profile, topN: 0));
    }

    private static bool ContainsAny(string hay, params string[] needles) =>
        needles.Any(n => hay.Contains(n, StringComparison.OrdinalIgnoreCase));
}
