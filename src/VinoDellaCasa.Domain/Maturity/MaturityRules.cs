using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Maturity;

/// <summary>
/// Pure, deterministic ReadyToDrink rules. No ML, no RP scores per château.
/// </summary>
public static class MaturityRules
{
    public static MaturityWindow WindowFor(MaturityStyle style) => style switch
    {
        MaturityStyle.PessacRougeAccessible => new(6, 10),
        MaturityStyle.PessacRougeSecondVin => new(6, 12),
        MaturityStyle.PessacRougeClasse => new(8, 16),
        MaturityStyle.PomerolMerlot => new(8, 18),
        MaturityStyle.StEmilionPremierGcc => new(7, 20),
        MaturityStyle.StEmilionGcc => new(6, 14),
        MaturityStyle.MedocStEstephe => new(8, 18),
        MaturityStyle.MedocSecondVin => new(5, 12),
        MaturityStyle.BlancSecBordeaux => new(3, 10),
        MaturityStyle.BlancSecSuisse => new(1, 5),
        MaturityStyle.RougeSuisse => new(2, 8),
        MaturityStyle.RougeGenerique => new(5, 12),
        MaturityStyle.BlancGenerique => new(1, 6),
        MaturityStyle.RoseOuEffervescent => new(0, 3),
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unknown maturity style.")
    };

    public static bool IsReadyToDrink(int? vintage, MaturityStyle style, int asOfYear)
    {
        if (vintage is null)
        {
            return false;
        }

        var age = asOfYear - vintage.Value;
        var w = WindowFor(style);
        return age >= w.PeakOpenAge && age <= w.PeakCloseAge;
    }

    /// <summary>
    /// MVP heuristic: first match wins. Seeds pin style explicitly via Notes keyword.
    /// </summary>
    public static MaturityStyle ResolveStyle(Bottle bottle)
    {
        ArgumentNullException.ThrowIfNull(bottle);

        var region = bottle.Region ?? string.Empty;
        var country = bottle.Country ?? string.Empty;
        var name = bottle.Name ?? string.Empty;
        var notes = bottle.Notes ?? string.Empty;
        var haystack = $"{name} {notes}";

        if (bottle.Color is Color.Rose or Color.Sparkling)
        {
            return MaturityStyle.RoseOuEffervescent;
        }

        if (bottle.Color == Color.White)
        {
            if (IsSwiss(country, region))
            {
                return MaturityStyle.BlancSecSuisse;
            }

            if (ContainsAny(region, "Pessac", "Graves", "Bordeaux"))
            {
                return MaturityStyle.BlancSecBordeaux;
            }

            return MaturityStyle.BlancGenerique;
        }

        if (ContainsAny(region, "Pomerol"))
        {
            return MaturityStyle.PomerolMerlot;
        }

        if (ContainsAny(region, "Saint-Émilion", "Saint-Emilion", "St-Emilion", "St Emilion"))
        {
            if (ContainsAny(haystack, "1er", "Premier Grand Cru"))
            {
                return MaturityStyle.StEmilionPremierGcc;
            }

            return MaturityStyle.StEmilionGcc;
        }

        if (ContainsAny(region, "Saint-Estèphe", "Saint-Estephe", "Pauillac", "Médoc", "Medoc", "Haut-Médoc", "Haut-Medoc", "Margaux", "Saint-Julien"))
        {
            if (ContainsAny(haystack, "2e vin", "2e", "Pagode", "second vin"))
            {
                return MaturityStyle.MedocSecondVin;
            }

            return MaturityStyle.MedocStEstephe;
        }

        if (ContainsAny(region, "Pessac-Léognan", "Pessac-Leognan", "Pessac", "Graves"))
        {
            if (ContainsAny(haystack, "2e", "Esprit", "second"))
            {
                return MaturityStyle.PessacRougeSecondVin;
            }

            if (ContainsAny(haystack, "Cru classé", "Cru classe", "Pape Clément", "Pape Clement", "Domaine de Chevalier"))
            {
                return MaturityStyle.PessacRougeClasse;
            }

            return MaturityStyle.PessacRougeAccessible;
        }

        if (bottle.Color == Color.Red && IsSwiss(country, region))
        {
            return MaturityStyle.RougeSuisse;
        }

        if (bottle.Color == Color.Red)
        {
            return MaturityStyle.RougeGenerique;
        }

        return MaturityStyle.BlancGenerique;
    }

    /// <summary>
    /// Prefer an explicit MaturityStyle=… token in Notes (used by M1 seeds); else heuristic.
    /// </summary>
    public static MaturityStyle ResolveStyleOrFromNotes(Bottle bottle)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        if (TryParseStyleFromNotes(bottle.Notes, out var pinned))
        {
            return pinned;
        }

        return ResolveStyle(bottle);
    }

    public static void ApplyReadyToDrink(Bottle bottle, int asOfYear, MaturityStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        var resolved = style ?? ResolveStyleOrFromNotes(bottle);
        bottle.ReadyToDrink = IsReadyToDrink(bottle.Vintage, resolved, asOfYear);
        bottle.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static bool TryParseStyleFromNotes(string? notes, out MaturityStyle style)
    {
        style = default;
        if (string.IsNullOrWhiteSpace(notes))
        {
            return false;
        }

        const string marker = "MaturityStyle=";
        var idx = notes.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return false;
        }

        var start = idx + marker.Length;
        var end = start;
        while (end < notes.Length && (char.IsLetterOrDigit(notes[end]) || notes[end] == '_'))
        {
            end++;
        }

        var token = notes[start..end];
        return Enum.TryParse(token, ignoreCase: true, out style);
    }

    private static bool IsSwiss(string country, string region) =>
        ContainsAny(country, "Suisse", "Switzerland", "Schweiz")
        || ContainsAny(region, "Valais", "Vaud", "Lavaux", "Genève", "Geneve", "Tessin", "Ticino", "Neuchâtel", "Neuchatel", "Trois Lacs", "Grisons");

    private static bool ContainsAny(string source, params string[] needles)
    {
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
