using VinoDellaCasa.Domain.Entities;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>One critic score chip for the fiche (facts only, no prose).</summary>
public sealed record CriticScoreChip(string Source, string Value)
{
    /// <summary>Chip label e.g. <c>RP 96</c> or <c>Hachette **</c>.</summary>
    public string Label => string.IsNullOrEmpty(Value) ? Source : $"{Source} {Value}";
}

/// <summary>Structured critic-score display helpers for catalogue fiches.</summary>
public static class CatalogScores
{
    /// <summary>
    /// Build RP / JS / WS / Hachette / ★ chips when present; empty when none
    /// (UI omits the block — graceful fallback).
    /// </summary>
    public static IReadOnlyList<CriticScoreChip> ForEntry(CatalogEntry? entry)
    {
        if (entry is null)
        {
            return [];
        }

        var chips = new List<CriticScoreChip>(5);

        if (entry.ScoreRp is int rp)
        {
            chips.Add(new CriticScoreChip("RP", rp.ToString()));
        }

        if (entry.ScoreJs is int js)
        {
            chips.Add(new CriticScoreChip("JS", js.ToString()));
        }

        if (entry.ScoreWs is int ws)
        {
            chips.Add(new CriticScoreChip("WS", ws.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(entry.ScoreHachette))
        {
            var mark = CatalogAssemblage.SanitizeText(entry.ScoreHachette);
            if (!string.IsNullOrEmpty(mark))
            {
                chips.Add(new CriticScoreChip("Hachette", mark));
            }
        }

        if (entry.Stars is int stars and > 0)
        {
            chips.Add(new CriticScoreChip("★", stars.ToString()));
        }

        return chips;
    }
}
