using System.Globalization;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain;

/// <summary>
/// Resolves a bottle to a Cave map region. See Docs/domain/MAP-REGION-MAPPING.md —
/// first match wins (table order); Country Suisse/Switzerland/Schweiz counts for Suisse.
/// </summary>
public static class WineMapRegions
{
    public static string Id(WineMapRegion region) => region switch
    {
        WineMapRegion.Bordeaux => "bordeaux",
        WineMapRegion.Bourgogne => "bourgogne",
        WineMapRegion.Champagne => "champagne",
        WineMapRegion.Loire => "loire",
        WineMapRegion.Alsace => "alsace",
        WineMapRegion.Rhone => "rhone",
        WineMapRegion.Provence => "provence",
        WineMapRegion.SudOuest => "sud-ouest",
        WineMapRegion.Suisse => "suisse",
        _ => "other"
    };

    public static string LabelFr(WineMapRegion region) => region switch
    {
        WineMapRegion.Bordeaux => "Bordeaux",
        WineMapRegion.Bourgogne => "Bourgogne",
        WineMapRegion.Champagne => "Champagne",
        WineMapRegion.Loire => "Loire",
        WineMapRegion.Alsace => "Alsace",
        WineMapRegion.Rhone => "Rhône",
        WineMapRegion.Provence => "Provence",
        WineMapRegion.SudOuest => "Sud-Ouest",
        WineMapRegion.Suisse => "Suisse",
        _ => "Autre"
    };

    /// <summary>Clickable map regions in display order (excludes Other).</summary>
    public static IReadOnlyList<WineMapRegion> MapHotspots { get; } =
    [
        WineMapRegion.Bordeaux,
        WineMapRegion.Bourgogne,
        WineMapRegion.Champagne,
        WineMapRegion.Loire,
        WineMapRegion.Alsace,
        WineMapRegion.Rhone,
        WineMapRegion.Provence,
        WineMapRegion.SudOuest,
        WineMapRegion.Suisse
    ];

    public static WineMapRegion Resolve(Bottle bottle)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        return Resolve(bottle.Region, bottle.Country);
    }

    public static WineMapRegion Resolve(string? region, string? country)
    {
        var hay = Normalize(region);
        var countryNorm = Normalize(country);

        // Table order — first match wins. Suisse also matches Country CH.
        // "bourg" (Côtes de Bourg) must not steal Bourgogne / Burgundy.
        if (ContainsAny(hay,
                "pessac", "léognan", "leognan", "graves", "pomerol", "saint-émilion", "saint-emilion",
                "médoc", "medoc", "haut-médoc", "haut-medoc", "pauillac", "margaux", "saint-julien",
                "saint-estèphe", "saint-estephe", "sauternes", "barsac", "fronsac", "castillon",
                "lalande", "blaye", "entre-deux-mers", "cadillac", "loupiac", "cérons",
                "cerons", "bordeaux")
            || (hay.Contains("bourg", StringComparison.Ordinal)
                && !hay.Contains("bourgogne", StringComparison.Ordinal)
                && !hay.Contains("burgundy", StringComparison.Ordinal)))
        {
            return WineMapRegion.Bordeaux;
        }

        if (ContainsAny(hay,
                "bourgogne", "burgundy", "côte de nuits", "cote de nuits", "côte de beaune",
                "cote de beaune", "chablis", "beaujolais", "mâcon", "macon", "côte chalonnaise",
                "cote chalonnaise"))
        {
            return WineMapRegion.Bourgogne;
        }

        if (ContainsAny(hay, "champagne", "reims", "épernay", "epernay"))
        {
            return WineMapRegion.Champagne;
        }

        if (ContainsAny(hay,
                "loire", "sancerre", "pouilly", "vouvray", "chinon", "bourgueil", "muscadet",
                "anjou", "saumur", "touraine"))
        {
            return WineMapRegion.Loire;
        }

        if (ContainsAny(hay, "alsace", "riesling", "gewurz", "colmar"))
        {
            return WineMapRegion.Alsace;
        }

        if (ContainsAny(hay,
                "rhône", "rhone", "côte-rôtie", "cote-rotie", "hermitage", "crozes",
                "châteauneuf", "chateauneuf", "gigondas", "condrieu", "cornas", "vacqueyras",
                "tavel"))
        {
            return WineMapRegion.Rhone;
        }

        if (ContainsAny(hay,
                "provence", "bandol", "côtes de provence", "cotes de provence",
                "coteaux d'aix", "coteaux d’aix", "palette", "cassis"))
        {
            return WineMapRegion.Provence;
        }

        if (ContainsAny(hay,
                "madiran", "cahors", "jurançon", "jurancon", "bergerac", "gaillac", "fronton",
                "marcillac", "irouléguy", "irouleguy", "buzet", "sud-ouest", "sud ouest"))
        {
            return WineMapRegion.SudOuest;
        }

        if (ContainsAny(hay,
                "valais", "vaud", "lavaux", "genève", "geneve", "tessin", "ticino",
                "neuchâtel", "neuchatel", "suisse", "switzerland")
            || countryNorm is "suisse" or "switzerland" or "schweiz")
        {
            return WineMapRegion.Suisse;
        }

        return WineMapRegion.Other;
    }

    public static bool TryParseId(string? id, out WineMapRegion region)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            region = WineMapRegion.Other;
            return false;
        }

        region = id.Trim().ToLowerInvariant() switch
        {
            "bordeaux" => WineMapRegion.Bordeaux,
            "bourgogne" => WineMapRegion.Bourgogne,
            "champagne" => WineMapRegion.Champagne,
            "loire" => WineMapRegion.Loire,
            "alsace" => WineMapRegion.Alsace,
            "rhone" or "rhône" => WineMapRegion.Rhone,
            "provence" => WineMapRegion.Provence,
            "sud-ouest" or "sudouest" => WineMapRegion.SudOuest,
            "suisse" => WineMapRegion.Suisse,
            "other" or "autre" => WineMapRegion.Other,
            _ => WineMapRegion.Other
        };

        return id.Trim().ToLowerInvariant() is "bordeaux" or "bourgogne" or "champagne" or "loire"
            or "alsace" or "rhone" or "rhône" or "provence" or "sud-ouest" or "sudouest"
            or "suisse" or "other" or "autre";
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLower(CultureInfo.InvariantCulture);

    private static bool ContainsAny(string hay, params string[] needles)
    {
        if (hay.Length == 0)
        {
            return false;
        }

        foreach (var n in needles)
        {
            if (hay.Contains(n, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
