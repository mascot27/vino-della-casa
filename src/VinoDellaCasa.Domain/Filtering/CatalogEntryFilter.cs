using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Filtering;

/// <summary>Pure catalogue browse filters (text, color, maturity). Compose with map region in UI.</summary>
public static class CatalogEntryFilter
{
    public static IEnumerable<CatalogEntry> Apply(
        IEnumerable<CatalogEntry> source,
        string? text = null,
        Color? color = null,
        bool? readyToDrink = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        IEnumerable<CatalogEntry> q = source;

        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim();
            q = q.Where(e =>
                Contains(e.Name, t)
                || Contains(e.Producer, t)
                || Contains(e.Appellation, t)
                || Contains(e.Style, t)
                || Contains(e.Pourquoi, t));
        }

        if (color is Color c)
        {
            q = q.Where(e => e.Color == c);
        }

        if (readyToDrink is bool ready)
        {
            q = q.Where(e => e.ReadyToDrink == ready);
        }

        return q;
    }

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrEmpty(haystack)
        && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
