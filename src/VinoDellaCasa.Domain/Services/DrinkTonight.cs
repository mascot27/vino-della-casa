using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;

namespace VinoDellaCasa.Domain.Services;

/// <summary>
/// Deterministic shortlist: Ready DESC, Vintage ASC, Name — no other M1 score.
/// </summary>
public static class DrinkTonight
{
    public static IReadOnlyList<Bottle> Shortlist(
        IEnumerable<Bottle> bottles,
        int topN = 5,
        int? asOfYear = null,
        bool recalculateReady = true)
    {
        ArgumentNullException.ThrowIfNull(bottles);
        if (topN < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(topN), topN, "topN must be >= 1.");
        }

        var year = asOfYear ?? DateTimeOffset.UtcNow.Year;
        var list = bottles.ToList();

        if (recalculateReady)
        {
            foreach (var bottle in list)
            {
                MaturityRules.ApplyReadyToDrink(bottle, year);
            }
        }

        return list
            .Where(b => b.Status != BottleStatus.Finished && b.Quantity > 0)
            .OrderByDescending(b => b.ReadyToDrink)
            .ThenBy(b => b.Vintage ?? int.MaxValue)
            .ThenBy(b => b.Name, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .ToList();
    }
}
