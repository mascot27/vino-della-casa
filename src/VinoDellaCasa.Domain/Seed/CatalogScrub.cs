using System.Text.RegularExpressions;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// Ensures catalogue / seed text stays anonymous — strips personal tags before publish.
/// </summary>
public static partial class CatalogScrub
{
    private static readonly string[] PersonalNeedles =
    [
        "corentin-",
        "corentin_",
        "corentin ",
        "wedding-",
        "wedding_",
        "notesinternal",
        "notes-internal",
        "notes_internal"
    ];

    /// <summary>True if haystack contains a personal / private marker (case-insensitive).</summary>
    public static bool ContainsPersonalMarker(string? haystack)
    {
        if (string.IsNullOrEmpty(haystack))
        {
            return false;
        }

        foreach (var n in PersonalNeedles)
        {
            if (haystack.Contains(n, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes personal style tags (corentin-*, wedding-*) and empty entries.
    /// </summary>
    public static IReadOnlyList<string> ScrubTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return Array.Empty<string>();
        }

        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t => !PersonalTagRegex().IsMatch(t.Trim()))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Returns null when text is clean; otherwise a short reason.</summary>
    public static string? FindPersonalLeak(string? text) =>
        ContainsPersonalMarker(text) ? "personal marker" : null;

    [GeneratedRegex(@"^(corentin|wedding)[-_].+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PersonalTagRegex();
}
