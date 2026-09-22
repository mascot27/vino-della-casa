using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Pairing;

/// <summary>
/// Builds a <see cref="WinePairingProfile"/> from catalogue / cellar entities.
/// When taste is missing, infers body/tannin/acidity/oak from color × region × style.
/// </summary>
public static class PairingProfileBuilder
{
    public static WinePairingProfile FromCatalog(CatalogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Taste is { } taste)
        {
            return new WinePairingProfile
            {
                Id = entry.Id,
                Name = entry.Name,
                Color = entry.Color,
                Region = entry.Appellation,
                Style = entry.Style,
                Blend = entry.Blend,
                ReadyToDrink = entry.ReadyToDrink,
                Body = taste.Body ?? InferBody(entry.Color, entry.Appellation, entry.Style, entry.Blend),
                Tannin = taste.Tannin ?? InferTannin(entry.Color, entry.Appellation, entry.Style, entry.Blend),
                Acidity = taste.Acidity ?? InferAcidity(entry.Color, entry.Appellation, entry.Style),
                Oak = taste.Oak ?? InferOak(entry.Color, entry.Appellation, entry.Style, entry.Classification),
                StyleTags = entry.StyleTags,
                PairingHints = MergeHints(taste.PairingHints, entry.Pairing)
            };
        }

        return InferFrom(
            entry.Id,
            entry.Name,
            entry.Color,
            entry.Appellation,
            entry.Style,
            entry.Blend,
            entry.Classification,
            entry.Pairing,
            entry.ReadyToDrink,
            entry.StyleTags);
    }

    public static WinePairingProfile FromBottle(Bottle bottle, string? style = null, string? pairing = null)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        return InferFrom(
            bottle.Id,
            bottle.Name,
            bottle.Color,
            bottle.Region,
            style,
            bottle.Varietal,
            classification: null,
            pairing,
            bottle.ReadyToDrink,
            styleTags: []);
    }

    public static WinePairingProfile InferFrom(
        Guid? id,
        string? name,
        Color color,
        string? region,
        string? style,
        string? blend,
        string? classification,
        string? pairing,
        bool readyToDrink,
        IReadOnlyList<string>? styleTags)
    {
        return new WinePairingProfile
        {
            Id = id,
            Name = name,
            Color = color,
            Region = region,
            Style = style,
            Blend = blend,
            ReadyToDrink = readyToDrink,
            Body = InferBody(color, region, style, blend),
            Tannin = InferTannin(color, region, style, blend),
            Acidity = InferAcidity(color, region, style),
            Oak = InferOak(color, region, style, classification),
            StyleTags = styleTags ?? [],
            PairingHints = SplitPairing(pairing)
        };
    }

    internal static TasteBody InferBody(Color color, string? region, string? style, string? blend)
    {
        var hay = Hay(region, style, blend);
        if (color == Color.Rose || color == Color.Sparkling)
        {
            return TasteBody.Light;
        }

        if (Contains(hay, "Beaujolais", "gamay", "léger", "light", "souple"))
        {
            return TasteBody.Light;
        }

        if (Contains(hay, "Grand Cru", "corps ample", "full", "musculaire", "de garde",
                "Pauillac", "Latour", "structure", "structuré"))
        {
            return TasteBody.Full;
        }

        if (Contains(hay, "Sauternes", "Barsac", "liquoreux"))
        {
            return TasteBody.Full;
        }

        if (Contains(hay, "Chablis") && Contains(hay, "Petit", "village"))
        {
            return TasteBody.Light;
        }

        if (Contains(hay, "Chablis"))
        {
            return Contains(hay, "Grand Cru", "Premier") ? TasteBody.Full : TasteBody.Medium;
        }

        if (Contains(hay, "Pomerol", "Saint-Émilion", "Saint-Emilion"))
        {
            return TasteBody.Medium;
        }

        if (Contains(hay, "Médoc", "Pauillac", "Saint-Estèphe", "cabernet"))
        {
            return TasteBody.Full;
        }

        if (Contains(style, "corps ample", "plein", "puissant"))
        {
            return TasteBody.Full;
        }

        if (Contains(style, "léger", "vif", "frais") && color == Color.White)
        {
            return TasteBody.Light;
        }

        return TasteBody.Medium;
    }

    internal static TasteTannin InferTannin(Color color, string? region, string? style, string? blend)
    {
        if (color != Color.Red)
        {
            return TasteTannin.Soft;
        }

        var hay = Hay(region, style, blend);
        if (Contains(hay, "Beaujolais", "gamay", "souple", "soft", "soyeux", "velouté", "Esprit", "Pagodes"))
        {
            return TasteTannin.Soft;
        }

        if (Contains(hay, "firm", "structuré", "tanin", "cabernet", "Pauillac", "Médoc",
                "Saint-Estèphe", "de garde", "musculaire"))
        {
            return TasteTannin.Firm;
        }

        if (Contains(hay, "Pomerol", "merlot", "Saint-Émilion", "Saint-Emilion"))
        {
            return TasteTannin.Medium;
        }

        if (Contains(hay, "Solitude", "structure souple"))
        {
            return TasteTannin.Soft;
        }

        return TasteTannin.Medium;
    }

    internal static TasteAcidity InferAcidity(Color color, string? region, string? style)
    {
        var hay = Hay(region, style);
        if (Contains(hay, "Chablis", "mineral", "minéral", "vif", "salin", "iodé", "flint", "high"))
        {
            return TasteAcidity.High;
        }

        if (color == Color.Sparkling || color == Color.Rose)
        {
            return TasteAcidity.High;
        }

        if (Contains(hay, "Sauternes", "Barsac", "liquoreux", "moelleux"))
        {
            return TasteAcidity.Medium;
        }

        if (color == Color.White)
        {
            return TasteAcidity.High;
        }

        return TasteAcidity.Medium;
    }

    internal static TasteOak InferOak(Color color, string? region, string? style, string? classification)
    {
        var hay = Hay(region, style, classification);
        if (Contains(hay, "sans bois", "none", "inox", "cuve"))
        {
            return TasteOak.None;
        }

        if (Contains(hay, "Meursault", "bois marqué", "marked", "élevage long", "Cru classé",
                "Grand Cru", "1er Grand Cru", "Pauillac"))
        {
            return color == Color.White || color == Color.Red ? TasteOak.Marked : TasteOak.Subtle;
        }

        if (Contains(hay, "Beaujolais", "gamay", "Chablis") && !Contains(hay, "Grand Cru"))
        {
            return TasteOak.None;
        }

        if (Contains(hay, "Chablis"))
        {
            return TasteOak.Subtle;
        }

        return TasteOak.Subtle;
    }

    private static IReadOnlyList<string> MergeHints(IReadOnlyList<string>? tasteHints, string? pairingLine)
    {
        var list = new List<string>();
        if (tasteHints is not null)
        {
            foreach (var h in tasteHints)
            {
                if (!string.IsNullOrWhiteSpace(h))
                {
                    list.Add(h.Trim());
                }
            }
        }

        foreach (var h in SplitPairing(pairingLine))
        {
            if (!list.Exists(x => string.Equals(x, h, StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(h);
            }
        }

        return list;
    }

    private static IReadOnlyList<string> SplitPairing(string? pairing)
    {
        if (string.IsNullOrWhiteSpace(pairing))
        {
            return [];
        }

        return pairing
            .Split(['·', ',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.Length > 0)
            .ToList();
    }

    private static string Hay(params string?[] parts) =>
        string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p)));

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
