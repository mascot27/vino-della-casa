using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// Anonymous catalogue seed (53 fiches). Built from the public demo sample + métier Style/Pourquoi.
/// Personal tags (corentin-*, wedding-*, notesInternal) are never present.
/// Images are local paths under wwwroot/img/catalog/ only.
/// </summary>
public static class CatalogSeedData
{
    public static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 18, 16, 0, 0, TimeSpan.Zero);

    private static readonly string[] CatalogImages =
    [
        "img/catalog/estate-chateau.jpg",
        "img/catalog/estate-cellar.jpg",
        "img/catalog/estate-barrels.jpg",
        "img/catalog/estate-vineyard.jpg"
    ];

    public static IReadOnlyList<CatalogEntry> CreateEntries(int asOfYear = 2026)
    {
        // Same identity/ids as DemoSample50 so « en cave » matches IndexedDB bottles by Id.
        var bottles = DemoSample50SeedData.CreateSeeds(asOfYear);
        var list = new List<CatalogEntry>(bottles.Count);

        for (var i = 0; i < bottles.Count; i++)
        {
            var b = bottles[i];
            var style = BuildStyle(b);
            var pourquoi = BuildPourquoi(b, asOfYear);
            var classification = InferClassification(b);
            var window = InferDrinkWindow(b);
            var pairing = InferPairing(b);

            // Scrub guard — seed must stay anonymous (no raw HTML / personal markers).
            if (CatalogScrub.ContainsPersonalMarker(style)
                || CatalogScrub.ContainsPersonalMarker(pourquoi)
                || CatalogScrub.ContainsPersonalMarker(b.Name)
                || CatalogScrub.ContainsPersonalMarker(b.Notes)
                || CatalogScrub.ContainsPersonalMarker(pairing)
                || CatalogScrub.ContainsPersonalMarker(classification)
                || CatalogScrub.ContainsPersonalMarker(b.Varietal))
            {
                throw new InvalidOperationException($"Personal marker leaked in catalog seed for {b.Name}.");
            }

            list.Add(new CatalogEntry
            {
                Id = b.Id,
                Name = b.Name,
                Producer = b.Producer,
                Appellation = b.Region ?? "—",
                Country = b.Country ?? "France",
                Color = b.Color,
                Vintage = b.Vintage,
                ImagePath = CatalogImages[i % CatalogImages.Length],
                Style = style,
                Pourquoi = pourquoi,
                Blend = b.Varietal,
                Classification = classification,
                Stars = null,
                DrinkWindow = window,
                Pairing = pairing,
                // Scores / price: only when factual numbers exist in seed — never hotlinked.
                ScoreRp = null,
                ScoreJs = null,
                ScoreHachette = null,
                PriceRangeChf = b.PriceChf is decimal p ? $"{p:0.##}" : null,
                ReadyToDrink = b.ReadyToDrink
            });
        }

        return list;
    }

    /// <summary>Lookup a catalogue fiche by stable seed Id (same Guid as demo bottles).</summary>
    public static CatalogEntry? FindById(Guid id, int asOfYear = 2026) =>
        CreateEntries(asOfYear).FirstOrDefault(e => e.Id == id);

    public const int ExpectedCount = 53;

    public static int Count => ExpectedCount;

    /// <summary>Maps a catalogue entry to a new cellar <see cref="Bottle"/> (qty set by caller).</summary>
    public static Bottle ToBottle(CatalogEntry entry, int quantity, int asOfYear = 2026)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var bottle = new Bottle
        {
            Id = entry.Id,
            Name = entry.Name,
            Producer = entry.Producer,
            Region = entry.Appellation,
            Country = entry.Country,
            Color = entry.Color,
            Varietal = entry.Blend,
            Vintage = entry.Vintage,
            Quantity = Math.Max(0, quantity),
            PriceChf = null,
            PurchaseDate = null,
            Bin = null,
            Status = quantity > 0 ? BottleStatus.InStock : BottleStatus.Finished,
            Notes = null,
            CreatedAt = SeedTimestamp,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        MaturityRules.ApplyReadyToDrink(bottle, asOfYear);
        return bottle;
    }

    internal static string BuildStyle(Bottle b)
    {
        var parts = new List<string>(3);

        if (b.Color == Color.Rose)
        {
            parts.Add("frais");
            parts.Add("fruité");
        }
        else if (b.Color == Color.Sparkling)
        {
            parts.Add("fines bulles");
            parts.Add("vif");
        }
        else if (b.Color == Color.White)
        {
            if (Contains(b.Region, "Sauternes", "Barsac"))
            {
                parts.Add("liquoreux");
                parts.Add("miel");
            }
            else if (Contains(b.Country, "Suisse") || Contains(b.Region, "Valais", "Vaud"))
            {
                parts.Add("floral");
                parts.Add("salin");
            }
            else
            {
                parts.Add("minéral");
                parts.Add("vif");
            }
        }
        else
        {
            // Rouge — short style from region heuristics (anonymous).
            if (Contains(b.Region, "Pomerol"))
            {
                parts.Add("velouté");
                parts.Add("merlot dominant");
            }
            else if (Contains(b.Region, "Saint-Émilion", "Saint-Emilion"))
            {
                parts.Add("fruité");
                parts.Add("élégant");
            }
            else if (Contains(b.Region, "Pauillac", "Saint-Estèphe", "Saint-Julien", "Médoc", "Haut-Médoc"))
            {
                parts.Add("structuré");
                parts.Add("cabernet");
            }
            else if (Contains(b.Region, "Margaux"))
            {
                parts.Add("floral");
                parts.Add("soyeux");
            }
            else if (Contains(b.Region, "Pessac", "Graves"))
            {
                if (Contains(b.Name, "Esprit", "Pagodes"))
                {
                    parts.Add("fruité");
                    parts.Add("structure souple");
                }
                else
                {
                    parts.Add("graviers");
                    parts.Add("fumé");
                }
            }
            else
            {
                parts.Add("fruité");
                parts.Add("structure souple");
            }
        }

        // Domaine de la Solitude mock: fruité · structure souple
        if (b.Name.Contains("Solitude", StringComparison.OrdinalIgnoreCase) && b.Color == Color.Red)
        {
            return "fruité · structure souple";
        }

        return string.Join(" · ", parts);
    }

    internal static string BuildPourquoi(Bottle b, int asOfYear)
    {
        var ready = b.ReadyToDrink;
        var age = b.Vintage is int v ? asOfYear - v : (int?)null;

        if (b.Color == Color.Rose)
        {
            return "À servir bien frais, profil estival et immédiat.";
        }

        if (b.Color == Color.Sparkling)
        {
            return ready
                ? "Prêt pour l’apéritif, bulles nettes et tension juste."
                : "Encore jeune — garder pour une belle occasion.";
        }

        if (Contains(b.Region, "Sauternes", "Barsac"))
        {
            return ready
                ? "Liquoreux prêt : dessert, foie gras, ou méditation."
                : "Encore en construction — la douceur gagnera en profondeur.";
        }

        if (Contains(b.Country, "Suisse") || Contains(b.Region, "Valais"))
        {
            return ready
                ? "Blanc de montagne prêt : pureté et tension pour la table."
                : "Jeune et nerveux — encore un peu de patience.";
        }

        if (ready)
        {
            if (age is >= 8)
            {
                return "Fenêtre ouverte : profondeur et tanins fondus, beau maintenant.";
            }

            return "Prêt à boire, bel équilibre pour ce soir.";
        }

        if (age is <= 4)
        {
            return "Encore jeune — laisser le fruit et la structure se rejoindre.";
        }

        return "À attendre : le pic de maturité n’est pas encore là.";
    }

    private static string? InferPairing(Bottle b)
    {
        if (b.Color == Color.Sparkling)
        {
            return "Apéritif · fruits de mer · fromages frais";
        }

        if (b.Color == Color.Rose)
        {
            return "Cuisine d’été · grillades légères · charcuterie";
        }

        if (b.Color == Color.White)
        {
            if (Contains(b.Region, "Sauternes", "Barsac"))
            {
                return "Foie gras · desserts aux fruits · fromages bleus";
            }

            if (Contains(b.Country, "Suisse") || Contains(b.Region, "Valais", "Vaud"))
            {
                return "Filets de perche · fromages alpins · cuisine de lac";
            }

            return "Poissons · volaille · fromages de chèvre";
        }

        // Rouge
        if (Contains(b.Region, "Pomerol", "Saint-Émilion", "Saint-Emilion"))
        {
            return "Agneau · canard · fromages à pâte molle";
        }

        if (Contains(b.Region, "Pauillac", "Saint-Estèphe", "Saint-Julien", "Médoc", "Haut-Médoc", "Margaux"))
        {
            return "Viandes rôties · gibier · fromages affinés";
        }

        if (Contains(b.Region, "Pessac", "Graves"))
        {
            return "Volaille rôtie · champignons · fromages à croûte fleurie";
        }

        return "Viandes · fromages · cuisine de saison";
    }

    private static string? InferClassification(Bottle b)
    {
        var name = b.Name ?? string.Empty;
        var region = b.Region ?? string.Empty;

        if (Contains(name, "Haut-Brion", "Latour", "Lafite", "Cheval Blanc", "Pétrus", "d'Yquem", "Angélus", "Pavie", "Figeac"))
        {
            return "Grand cru / icône";
        }

        if (Contains(region, "Saint-Émilion", "Saint-Emilion") && Contains(name, "Premier", "1er"))
        {
            return "Premier Grand Cru Classé";
        }

        if (Contains(region, "Pessac") && Contains(name, "Chevalier", "Pape Clément", "Haut-Bailly", "Smith Haut", "Malartic", "Carbonnieux"))
        {
            return "Cru classé de Graves";
        }

        if (Contains(name, "Esprit", "Pagodes"))
        {
            return "Second vin";
        }

        return null;
    }

    private static string? InferDrinkWindow(Bottle b)
    {
        var style = MaturityRules.ResolveStyleOrFromNotes(b);
        var w = MaturityRules.WindowFor(style);
        if (b.Vintage is not int v)
        {
            return null;
        }

        return $"{v + w.PeakOpenAge}–{v + w.PeakCloseAge}";
    }

    private static bool Contains(string? source, params string[] needles)
    {
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }

        foreach (var n in needles)
        {
            if (source.Contains(n, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
