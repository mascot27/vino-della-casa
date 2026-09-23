using System.Text.Json;
using System.Text.Json.Serialization;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;
using VinoDellaCasa.Domain.Pairing;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// Anonymous catalogue seed (53 demo + 100 Bourgogne). Built from the public demo sample +
/// <c>Docs/domain/catalog-bourgogne-100.json</c>. Personal tags are never present.
/// Images are local paths under wwwroot/img/catalog/ only (null imageKey → estate placeholders).
/// </summary>
public static class CatalogSeedData
{
    public static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 18, 16, 0, 0, TimeSpan.Zero);

    public const int DemoCount = 53;
    public const int BourgogneCount = 100;
    public const int ExpectedCount = DemoCount + BourgogneCount;

    private const string BourgogneResourceName = "VinoDellaCasa.Domain.Seed.catalog-bourgogne-100.json";
    private const int BourgogneAsOfYear = 2026;

    private static readonly string[] CatalogImages =
    [
        "img/catalog/estate-chateau.jpg",
        "img/catalog/estate-cellar.jpg",
        "img/catalog/estate-barrels.jpg",
        "img/catalog/estate-vineyard.jpg"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly Lazy<IReadOnlyList<BourgogneBottleDto>> BourgogneDtos =
        new(LoadBourgogneDtos);

    public static IReadOnlyList<CatalogEntry> CreateEntries(int asOfYear = 2026)
    {
        // Same identity/ids as DemoSample50 so « en cave » matches IndexedDB bottles by Id.
        var bottles = DemoSample50SeedData.CreateSeeds(asOfYear);
        var list = new List<CatalogEntry>(ExpectedCount);

        for (var i = 0; i < bottles.Count; i++)
        {
            var b = bottles[i];
            var style = BuildStyle(b);
            var pourquoi = BuildPourquoi(b, asOfYear);
            var classification = InferClassification(b);
            var window = InferDrinkWindow(b);
            var pairing = InferPairing(b);

            GuardAnonymous(b.Name, style, pourquoi, b.Notes, pairing, classification, b.Varietal);

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
                BlendsByVintage = CatalogDemoBlends.ForName(b.Name),
                Classification = classification,
                Stars = null,
                DrinkWindow = window,
                Pairing = pairing,
                ScoreRp = null,
                ScoreJs = null,
                ScoreWs = null,
                ScoreHachette = null,
                PriceRangeChf = b.PriceChf is decimal p ? $"{p:0.##}" : null,
                ReadyToDrink = b.ReadyToDrink,
                StyleTags = [],
                Taste = null
            });
        }

        var bgStart = list.Count;
        for (var i = 0; i < BourgogneDtos.Value.Count; i++)
        {
            list.Add(MapBourgogne(BourgogneDtos.Value[i], bgStart + i, asOfYear));
        }

        if (list.Count != ExpectedCount)
        {
            throw new InvalidOperationException(
                $"Catalog seed count mismatch: expected {ExpectedCount}, got {list.Count}.");
        }

        if (list.Select(e => e.Id).Distinct().Count() != list.Count)
        {
            throw new InvalidOperationException("Catalog seed ids are not unique.");
        }

        return list;
    }

    /// <summary>Lookup a catalogue fiche by stable seed Id (same Guid as demo bottles / bg keys).</summary>
    public static CatalogEntry? FindById(Guid id, int asOfYear = 2026) =>
        CreateEntries(asOfYear).FirstOrDefault(e => e.Id == id);

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

    private static CatalogEntry MapBourgogne(BourgogneBottleDto dto, int imageIndex, int asOfYear)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            throw new InvalidOperationException("Bourgogne seed entry missing id.");
        }

        var style = string.IsNullOrWhiteSpace(dto.StyleLine) ? "fruité · structure souple" : dto.StyleLine.Trim();
        var pourquoi = string.IsNullOrWhiteSpace(dto.Pourquoi)
            ? "Profil Bourgogne — laisser parler le climat et le cépage."
            : dto.Pourquoi.Trim();
        var blend = FirstNonEmpty(dto.Assemblage, dto.Varietal);
        var classification = string.IsNullOrWhiteSpace(dto.Classification) ? null : dto.Classification.Trim();
        var pairing = FormatPairing(dto.Taste?.PairingHints);
        var window = FormatDrinkWindow(dto.Vintage, dto.Taste?.DrinkWindowYears);
        var imagePath = ResolveImagePath(dto.ImageKey, imageIndex);
        var ready = ResolveReady(dto, asOfYear);

        GuardAnonymous(dto.Name, style, pourquoi, dto.Notes, pairing, classification, blend, dto.Producer);

        return new CatalogEntry
        {
            Id = CatalogSeedIds.FromKey(dto.Id),
            Name = dto.Name.Trim(),
            Producer = string.IsNullOrWhiteSpace(dto.Producer) ? null : dto.Producer.Trim(),
            Appellation = string.IsNullOrWhiteSpace(dto.Region) ? "Bourgogne" : dto.Region.Trim(),
            Country = string.IsNullOrWhiteSpace(dto.Country) ? "France" : dto.Country.Trim(),
            Color = ParseColor(dto.Color),
            Vintage = dto.Vintage,
            ImagePath = imagePath,
            Style = style,
            Pourquoi = pourquoi,
            Blend = blend,
            BlendsByVintage = MapBlendsByVintage(dto.BlendsByVintage),
            Classification = classification,
            Stars = null,
            DrinkWindow = window,
            Pairing = pairing,
            ScoreRp = dto.Scores?.Rp,
            ScoreJs = dto.Scores?.Js,
            ScoreWs = dto.Scores?.Ws,
            ScoreHachette = FormatHachette(dto.Scores?.HachetteStars),
            PriceRangeChf = dto.PriceChf is decimal p ? $"{p:0.##}" : null,
            ReadyToDrink = ready,
            StyleTags = MapStyleTags(dto.StyleTags),
            Taste = MapTaste(dto.Taste)
        };
    }

    private static bool ResolveReady(BourgogneBottleDto dto, int asOfYear)
    {
        if (dto.Taste?.DrinkWindowYears is { } w && dto.Vintage is int v)
        {
            var age = asOfYear - v;
            return age >= w.Open && age <= w.Close;
        }

        if (asOfYear == BourgogneAsOfYear)
        {
            return dto.ReadyToDrink;
        }

        return dto.ReadyToDrink;
    }

    private static string ResolveImagePath(string? imageKey, int imageIndex)
    {
        if (!string.IsNullOrWhiteSpace(imageKey)
            && imageKey.StartsWith("img/catalog/", StringComparison.Ordinal)
            && !imageKey.Contains("://", StringComparison.Ordinal))
        {
            return imageKey;
        }

        return CatalogImages[Math.Abs(imageIndex) % CatalogImages.Length];
    }

    private static Color ParseColor(string? color) =>
        Enum.TryParse<Color>(color, ignoreCase: true, out var c) ? c : Color.Other;

    private static string? FormatPairing(IReadOnlyList<string>? hints)
    {
        if (hints is null || hints.Count == 0)
        {
            return null;
        }

        var parts = hints
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Select(h => h.Trim())
            .ToList();
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string? FormatDrinkWindow(int? vintage, DrinkWindowYearsDto? window)
    {
        if (vintage is not int v || window is null)
        {
            return null;
        }

        return $"{v + window.Open}–{v + window.Close}";
    }

    private static string? FormatHachette(int? stars) =>
        stars is int s and > 0 ? new string('*', Math.Min(s, 5)) : null;

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v.Trim();
            }
        }

        return null;
    }

    private static void GuardAnonymous(params string?[] fields)
    {
        foreach (var f in fields)
        {
            if (CatalogScrub.ContainsPersonalMarker(f))
            {
                throw new InvalidOperationException($"Personal marker leaked in catalog seed: {f}");
            }
        }
    }

    private static IReadOnlyList<BourgogneBottleDto> LoadBourgogneDtos()
    {
        using var stream = typeof(CatalogSeedData).Assembly.GetManifestResourceStream(BourgogneResourceName)
            ?? throw new InvalidOperationException(
                $"Missing embedded resource '{BourgogneResourceName}'.");

        var file = JsonSerializer.Deserialize<BourgogneCatalogFile>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Bourgogne catalog JSON deserialized to null.");

        if (file.Bottles.Count != BourgogneCount)
        {
            throw new InvalidOperationException(
                $"Bourgogne catalog expected {BourgogneCount} bottles, got {file.Bottles.Count}.");
        }

        return file.Bottles;
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


    private static IReadOnlyList<string> MapStyleTags(IReadOnlyList<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return [];
        }

        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToList();
    }

    private static WineTaste? MapTaste(BourgogneTasteDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        var hints = dto.PairingHints?
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Select(h => h.Trim())
            .ToList() ?? [];

        return new WineTaste
        {
            Body = ParseBody(dto.Body),
            Tannin = ParseTannin(dto.Tannin),
            Acidity = ParseAcidity(dto.Acidity),
            Oak = ParseOak(dto.Oak),
            PairingHints = hints
        };
    }

    private static TasteBody? ParseBody(string? value) =>
        Enum.TryParse<TasteBody>(value, ignoreCase: true, out var v) ? v : null;

    private static TasteTannin? ParseTannin(string? value) =>
        Enum.TryParse<TasteTannin>(value, ignoreCase: true, out var v) ? v : null;

    private static TasteAcidity? ParseAcidity(string? value) =>
        Enum.TryParse<TasteAcidity>(value, ignoreCase: true, out var v) ? v : null;

    private static TasteOak? ParseOak(string? value) =>
        Enum.TryParse<TasteOak>(value, ignoreCase: true, out var v) ? v : null;

    private static IReadOnlyList<BlendByVintage> MapBlendsByVintage(
        IReadOnlyList<BlendByVintageDto>? dtos)
    {
        if (dtos is null || dtos.Count == 0)
        {
            return [];
        }

        var mapped = new List<BlendByVintage>(dtos.Count);
        foreach (var dto in dtos)
        {
            if (dto.Vintage is not int vintage)
            {
                continue;
            }

            var components = (dto.Components ?? [])
                .Select(c => new BlendComponent
                {
                    Grape = CatalogAssemblage.SanitizeText(c.Grape),
                    Percent = c.Percent
                })
                .Where(c => !string.IsNullOrEmpty(c.Grape))
                .ToList();

            if (components.Count == 0)
            {
                continue;
            }

            mapped.Add(new BlendByVintage
            {
                Vintage = vintage,
                Components = components
            });
        }

        return CatalogAssemblage.MapFromSeed(mapped);
    }

    private sealed class BlendByVintageDto
    {
        public int? Vintage { get; set; }
        public List<BlendComponentDto>? Components { get; set; }
    }

    private sealed class BlendComponentDto
    {
        public string? Grape { get; set; }
        public int? Percent { get; set; }
    }

    private sealed class BourgogneCatalogFile
    {
        public List<BourgogneBottleDto> Bottles { get; set; } = [];
    }

    private sealed class BourgogneBottleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Producer { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Color { get; set; }
        public string? Varietal { get; set; }
        public int? Vintage { get; set; }
        public decimal? PriceChf { get; set; }
        public string? Notes { get; set; }
        public bool ReadyToDrink { get; set; }
        public string? StyleLine { get; set; }
        public string? Pourquoi { get; set; }
        public string? Classification { get; set; }
        public string? Assemblage { get; set; }
        /// <summary>Optional wine-knowledge blends (% by vintage); empty until data exists.</summary>
        public List<BlendByVintageDto>? BlendsByVintage { get; set; }
        public string? ImageKey { get; set; }
        public List<string>? StyleTags { get; set; }
        public BourgogneScoresDto? Scores { get; set; }
        public BourgogneTasteDto? Taste { get; set; }
    }

    private sealed class BourgogneScoresDto
    {
        public int? Rp { get; set; }
        public int? Js { get; set; }
        public int? Ws { get; set; }
        public int? HachetteStars { get; set; }
    }

    private sealed class BourgogneTasteDto
    {
        public string? Body { get; set; }
        public string? Tannin { get; set; }
        public string? Acidity { get; set; }
        public string? Oak { get; set; }
        public DrinkWindowYearsDto? DrinkWindowYears { get; set; }
        public List<string>? PairingHints { get; set; }
    }

    private sealed class DrinkWindowYearsDto
    {
        public int Open { get; set; }
        public int Close { get; set; }
    }
}
