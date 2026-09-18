using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Filtering;

/// <summary>Pure Cave browse filters (text, color, maturity, bin). Compose with map region in UI.</summary>
public static class CaveBottleFilter
{
    public static IEnumerable<Bottle> Apply(
        IEnumerable<Bottle> source,
        string? text = null,
        Color? color = null,
        bool? readyToDrink = null,
        string? bin = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        IEnumerable<Bottle> q = source;

        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim();
            q = q.Where(b =>
                Contains(b.Name, t)
                || Contains(b.Producer, t)
                || Contains(b.Region, t));
        }

        if (color is Color c)
        {
            q = q.Where(b => b.Color == c);
        }

        if (readyToDrink is bool ready)
        {
            q = q.Where(b => b.ReadyToDrink == ready);
        }

        if (!string.IsNullOrWhiteSpace(bin))
        {
            var needle = bin.Trim();
            q = q.Where(b => Contains(b.Bin, needle));
        }

        return q;
    }

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrEmpty(haystack)
        && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
