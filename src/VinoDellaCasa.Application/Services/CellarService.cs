using VinoDellaCasa.Application.Abstractions;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Maturity;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Domain.Services;

namespace VinoDellaCasa.Application.Services;

/// <summary>
/// Application façade over <see cref="ICellarStore"/>: CRUD, seed, drink-tonight.
/// Recalculates ReadyToDrink via domain maturity rules before shortlists / writes.
/// </summary>
public sealed class CellarService
{
    private readonly ICellarStore _store;

    public CellarService(ICellarStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Bottle>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _store.GetAllAsync(cancellationToken);

    public Task<Bottle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _store.GetByIdAsync(id, cancellationToken);

    public async Task<Bottle> AddAsync(Bottle bottle, int? asOfYear = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        MaturityRules.ApplyReadyToDrink(bottle, asOfYear ?? DateTimeOffset.UtcNow.Year);
        return await _store.AddAsync(bottle, cancellationToken);
    }

    public async Task<Bottle> UpdateAsync(Bottle bottle, int? asOfYear = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bottle);
        MaturityRules.ApplyReadyToDrink(bottle, asOfYear ?? DateTimeOffset.UtcNow.Year);
        return await _store.UpdateAsync(bottle, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        _store.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Bottle>> GetDrinkTonightAsync(
        int topN = 5,
        int? asOfYear = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _store.GetAllAsync(cancellationToken);
        return DrinkTonight.Shortlist(all, topN, asOfYear, recalculateReady: true);
    }

    /// <summary>
    /// Loads M1 SampleBordeaux + CH seeds when the cellar is empty (or forceReplace).
    /// </summary>
    public async Task<int> EnsureM1SeedsAsync(
        int asOfYear = 2026,
        bool forceReplace = false,
        CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAllAsync(cancellationToken);
        if (existing.Count > 0 && !forceReplace)
        {
            return 0;
        }

        if (forceReplace)
        {
            foreach (var b in existing)
            {
                await _store.DeleteAsync(b.Id, cancellationToken);
            }
        }

        var seeds = M1SeedData.CreateM1Seeds(asOfYear);
        foreach (var seed in seeds)
        {
            await _store.AddAsync(seed, cancellationToken);
        }

        return seeds.Count;
    }
}
