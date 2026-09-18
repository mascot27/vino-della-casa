using VinoDellaCasa.Application.Abstractions;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Domain.Services;

namespace VinoDellaCasa.Application.Services;

/// <summary>
/// Application façade over <see cref="ICellarStore"/>: CRUD, seed, drink-tonight, catalogue qty.
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
    /// Loads anonymous demo seeds (53) when the cellar is empty (or forceReplace).
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

        var seeds = DemoSample50SeedData.CreateSeeds(asOfYear);
        foreach (var seed in seeds)
        {
            await _store.AddAsync(seed, cancellationToken);
        }

        return seeds.Count;
    }

    /// <summary>
    /// Upserts cellar ownership for a catalogue entry (IndexedDB qty). qty ≤ 0 removes the bottle.
    /// Matches by Id first, then Name + Vintage.
    /// </summary>
    public async Task<Bottle?> SetCatalogQuantityAsync(
        CatalogEntry entry,
        int quantity,
        int? asOfYear = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var year = asOfYear ?? DateTimeOffset.UtcNow.Year;
        var existing = await FindMatchingBottleAsync(entry, cancellationToken);

        if (quantity <= 0)
        {
            if (existing is not null)
            {
                await _store.DeleteAsync(existing.Id, cancellationToken);
            }

            return null;
        }

        if (existing is null)
        {
            var created = CatalogSeedData.ToBottle(entry, quantity, year);
            return await AddAsync(created, year, cancellationToken);
        }

        existing.Quantity = quantity;
        existing.Status = BottleStatus.InStock;
        existing.Name = entry.Name;
        existing.Producer = entry.Producer;
        existing.Region = entry.Appellation;
        existing.Country = entry.Country;
        existing.Color = entry.Color;
        existing.Varietal = entry.Blend;
        existing.Vintage = entry.Vintage;
        return await UpdateAsync(existing, year, cancellationToken);
    }

    private async Task<Bottle?> FindMatchingBottleAsync(CatalogEntry entry, CancellationToken cancellationToken)
    {
        var byId = await _store.GetByIdAsync(entry.Id, cancellationToken);
        if (byId is not null)
        {
            return byId;
        }

        var all = await _store.GetAllAsync(cancellationToken);
        return all.FirstOrDefault(b =>
            string.Equals(b.Name, entry.Name, StringComparison.OrdinalIgnoreCase)
            && b.Vintage == entry.Vintage);
    }
}
