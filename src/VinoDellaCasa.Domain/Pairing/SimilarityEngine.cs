using VinoDellaCasa.Domain;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Pairing;

/// <summary>
/// Deterministic style similarity (Docs/domain/SIMILARITY-RULES.md). Offline, no ML.
/// </summary>
public static class SimilarityEngine
{
    public const int DefaultTopN = 5;

    public static IReadOnlyList<SimilarityMatch> FindSimilar(
        WinePairingProfile source,
        IEnumerable<WinePairingProfile> candidates,
        int topN = DefaultTopN)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidates);
        if (topN < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(topN), topN, "topN must be >= 1.");
        }

        var scored = new List<SimilarityMatch>();
        foreach (var candidate in candidates)
        {
            if (candidate is null)
            {
                continue;
            }

            if (source.Id is Guid sid && candidate.Id is Guid cid && sid == cid)
            {
                continue;
            }

            if (!TryScore(source, candidate, out var score, out var why))
            {
                continue;
            }

            scored.Add(new SimilarityMatch(candidate, score, why));
        }

        return scored
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Profile.Name, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .ToList();
    }

    /// <summary>Returns false when the candidate is excluded (hard color mismatch).</summary>
    internal static bool TryScore(
        WinePairingProfile source,
        WinePairingProfile candidate,
        out int score,
        out string why)
    {
        score = 0;
        why = string.Empty;

        var colorPts = ScoreColor(source.Color, candidate.Color, out var colorExcluded);
        if (colorExcluded)
        {
            return false;
        }

        var bodyPts = AdjacentScore((int)source.Body, (int)candidate.Body, exact: 20, adjacent: 10);
        var tanninPts = source.Color == Color.Red && candidate.Color == Color.Red
            ? AdjacentScore((int)source.Tannin, (int)candidate.Tannin, exact: 15, adjacent: 8)
            : 0;
        var acidityPts = AdjacentScore((int)source.Acidity, (int)candidate.Acidity, exact: 15, adjacent: 8);
        var oakPts = AdjacentScore((int)source.Oak, (int)candidate.Oak, exact: 10, adjacent: 5);
        var regionPts = ScoreRegion(source.Region, candidate.Region, out var regionKind);
        var tagPts = ScoreStyleTags(source.StyleTags, candidate.StyleTags, out var sharedTags);

        var total = colorPts + bodyPts + tanninPts + acidityPts + oakPts + regionPts + tagPts;

        if (source.Vintage is int sv && candidate.Vintage is int cv && Math.Abs(sv - cv) > 8)
        {
            total -= 5;
        }

        if (source.ReadyToDrink && !candidate.ReadyToDrink)
        {
            total -= 5;
        }

        score = Math.Clamp(total, 0, 100);
        why = BuildWhy(source, candidate, colorPts, bodyPts, tanninPts, regionKind, sharedTags);
        return true;
    }

    private static int ScoreColor(Color a, Color b, out bool excluded)
    {
        if (a == b)
        {
            excluded = false;
            return 30;
        }

        if (IsSparklingWhitePair(a, b))
        {
            excluded = false;
            return 10;
        }

        excluded = true;
        return 0;
    }

    private static bool IsSparklingWhitePair(Color a, Color b) =>
        (a == Color.Sparkling && b == Color.White) || (a == Color.White && b == Color.Sparkling);

    private static int AdjacentScore(int a, int b, int exact, int adjacent)
    {
        var d = Math.Abs(a - b);
        if (d == 0)
        {
            return exact;
        }

        if (d == 1)
        {
            return adjacent;
        }

        return 0;
    }

    private static int ScoreRegion(string? sourceRegion, string? candidateRegion, out string regionKind)
    {
        regionKind = "none";
        if (string.IsNullOrWhiteSpace(sourceRegion) || string.IsNullOrWhiteSpace(candidateRegion))
        {
            return 0;
        }

        if (string.Equals(sourceRegion.Trim(), candidateRegion.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            regionKind = "appellation";
            return 15;
        }

        var srcHot = WineMapRegions.Resolve(sourceRegion, country: null);
        var candHot = WineMapRegions.Resolve(candidateRegion, country: null);
        if (srcHot != WineMapRegion.Other && srcHot == candHot)
        {
            regionKind = "hotspot";
            return 10;
        }

        return 0;
    }

    private static int ScoreStyleTags(
        IReadOnlyList<string> sourceTags,
        IReadOnlyList<string> candidateTags,
        out List<string> shared)
    {
        shared = [];
        if (sourceTags.Count == 0 || candidateTags.Count == 0)
        {
            return 0;
        }

        var cand = new HashSet<string>(
            candidateTags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()),
            StringComparer.OrdinalIgnoreCase);

        foreach (var tag in sourceTags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            var t = tag.Trim();
            if (cand.Contains(t) && !shared.Exists(x => string.Equals(x, t, StringComparison.OrdinalIgnoreCase)))
            {
                shared.Add(t);
            }
        }

        return Math.Min(10, shared.Count * 5);
    }

    private static string BuildWhy(
        WinePairingProfile source,
        WinePairingProfile candidate,
        int colorPts,
        int bodyPts,
        int tanninPts,
        string regionKind,
        IReadOnlyList<string> sharedTags)
    {
        // Prefer evocative templates when tags / region signal a known style.
        if (sharedTags.Any(t => t.Contains("chablis", StringComparison.OrdinalIgnoreCase)
                                || t.Contains("mineral", StringComparison.OrdinalIgnoreCase)
                                || t.Contains("minéral", StringComparison.OrdinalIgnoreCase))
            || Contains(source.Region, "Chablis") || Contains(candidate.Region, "Chablis"))
        {
            return "Style Chablis / minéral voisin";
        }

        if (regionKind is "appellation" or "hotspot")
        {
            var family = WineMapRegions.LabelFr(WineMapRegions.Resolve(source.Region, null));
            if (regionKind == "appellation" && !string.IsNullOrWhiteSpace(source.Region))
            {
                return $"Même appellation ({source.Region.Trim()}), profil proche";
            }

            if (family is not "Autre")
            {
                return $"Même famille {family}, profil proche";
            }
        }

        if (colorPts >= 30 && (bodyPts >= 10 || tanninPts >= 8))
        {
            var structure = DescribeStructure(source);
            return string.IsNullOrEmpty(structure)
                ? "Même couleur et structure proche"
                : $"Même couleur et structure ({structure})";
        }

        if (colorPts >= 30)
        {
            var hotspot = WineMapRegions.LabelFr(WineMapRegions.Resolve(candidate.Region, null));
            return hotspot is "Autre"
                ? "Même couleur, structure proche"
                : $"Même couleur, structure proche, {hotspot}";
        }

        return "Profil de goût voisin";
    }

    private static string DescribeStructure(WinePairingProfile p)
    {
        var parts = new List<string>();
        parts.Add(p.Body switch
        {
            TasteBody.Light => "corps léger",
            TasteBody.Full => "corps plein",
            _ => "corps moyen"
        });

        if (p.Color == Color.Red)
        {
            parts.Add(p.Tannin switch
            {
                TasteTannin.Soft => "tanins souples",
                TasteTannin.Firm => "tanins fermes",
                _ => "tanins moyens"
            });
        }
        else if (p.Acidity == TasteAcidity.High)
        {
            parts.Add("acidité vive");
        }

        return string.Join(", ", parts);
    }

    private static bool Contains(string? source, string needle) =>
        !string.IsNullOrEmpty(source) && source.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
