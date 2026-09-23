using System.Text.RegularExpressions;
using VinoDellaCasa.Domain.Entities;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// Assemblage display helpers: prefer fiche vintage, chip labels, sanitize seed text.
/// </summary>
public static partial class CatalogAssemblage
{
    /// <summary>
    /// Prefer the blend matching <paramref name="ficheVintage"/>; if absent and exactly one
    /// vintage row has components, use that. Multi-vintage without a fiche match → null
    /// (UI uses <see cref="ListForFiche"/> to show every millésime).
    /// </summary>
    public static BlendByVintage? PreferForFiche(
        IReadOnlyList<BlendByVintage>? blends,
        int? ficheVintage)
    {
        var usable = ListForFiche(blends, ficheVintage);
        if (usable.Count == 0)
        {
            return null;
        }

        if (ficheVintage is int v)
        {
            var match = usable.FirstOrDefault(b => b.Vintage == v);
            if (match is not null)
            {
                return match;
            }
        }

        return usable.Count == 1 ? usable[0] : null;
    }

    /// <summary>
    /// All usable <c>blendsByVintage</c> rows for the fiche: matching millésime first,
    /// then remaining by vintage descending. Empty when absent (plain Blend fallback).
    /// </summary>
    public static IReadOnlyList<BlendByVintage> ListForFiche(
        IReadOnlyList<BlendByVintage>? blends,
        int? ficheVintage)
    {
        if (blends is null || blends.Count == 0)
        {
            return [];
        }

        var usable = blends
            .Where(b => b.Components.Count > 0)
            .Select(Normalize)
            .Where(b => b.Components.Count > 0)
            .ToList();

        if (usable.Count == 0)
        {
            return [];
        }

        return usable
            .OrderByDescending(b => ficheVintage is int v && b.Vintage == v)
            .ThenByDescending(b => b.Vintage)
            .ToList();
    }

    /// <summary>Chip text e.g. <c>Merlot 60%</c> or grape alone when percent missing.</summary>
    public static string FormatChip(BlendComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        var grape = SanitizeText(component.Grape);
        if (string.IsNullOrEmpty(grape))
        {
            return string.Empty;
        }

        return component.Percent is int p ? $"{grape} {p}%" : grape;
    }

    /// <summary>Strip tags / angle brackets so seed text never becomes markup.</summary>
    public static string SanitizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        var noTags = HtmlTagRegex().Replace(trimmed, string.Empty);
        return noTags.Replace("<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal)
            .Trim();
    }

    /// <summary>Map seed rows → domain (sanitized, drops empty grapes).</summary>
    public static IReadOnlyList<BlendByVintage> MapFromSeed(
        IEnumerable<BlendByVintage>? source)
    {
        if (source is null)
        {
            return Array.Empty<BlendByVintage>();
        }

        return source
            .Select(Normalize)
            .Where(b => b.Components.Count > 0)
            .ToList();
    }

    private static BlendByVintage Normalize(BlendByVintage blend)
    {
        var components = blend.Components
            .Select(c => new BlendComponent
            {
                Grape = SanitizeText(c.Grape),
                Percent = c.Percent
            })
            .Where(c => !string.IsNullOrEmpty(c.Grape))
            .ToList();

        return new BlendByVintage
        {
            Vintage = blend.Vintage,
            Components = components
        };
    }

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();
}
