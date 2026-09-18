using System.Collections.Concurrent;
using VinoDellaCasa.Application.Abstractions;
using VinoDellaCasa.Domain.Entities;

namespace VinoDellaCasa.Infrastructure.Persistence;

/// <summary>
/// In-memory / test double for CI and unit tests.
/// Browser production path: <see cref="IndexedDbCellarStore"/> (IndexedDB via JS interop).
/// </summary>
public sealed class InMemoryCellarStore : ICellarStore
{
    private readonly ConcurrentDictionary<Guid, Bottle> _bottles = new();

    public Task<IReadOnlyList<Bottle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Bottle> list = _bottles.Values.OrderBy(b => b.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<Bottle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _bottles.TryGetValue(id, out var bottle);
        return Task.FromResult(bottle);
    }

    public Task<Bottle> AddAsync(Bottle bottle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(bottle);
        if (bottle.Id == Guid.Empty)
        {
            bottle.Id = Guid.NewGuid();
        }

        var now = DateTimeOffset.UtcNow;
        if (bottle.CreatedAt == default)
        {
            bottle.CreatedAt = now;
        }

        bottle.UpdatedAt = now;

        if (!_bottles.TryAdd(bottle.Id, Clone(bottle)))
        {
            throw new InvalidOperationException($"Bottle with id {bottle.Id} already exists.");
        }

        return Task.FromResult(Clone(bottle));
    }

    public Task<Bottle> UpdateAsync(Bottle bottle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(bottle);

        if (!_bottles.ContainsKey(bottle.Id))
        {
            throw new KeyNotFoundException($"Bottle with id {bottle.Id} was not found.");
        }

        bottle.UpdatedAt = DateTimeOffset.UtcNow;
        _bottles[bottle.Id] = Clone(bottle);
        return Task.FromResult(Clone(bottle));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_bottles.TryRemove(id, out _));
    }

    public void Clear() => _bottles.Clear();

    private static Bottle Clone(Bottle b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Producer = b.Producer,
        Region = b.Region,
        Country = b.Country,
        Color = b.Color,
        Varietal = b.Varietal,
        Vintage = b.Vintage,
        Quantity = b.Quantity,
        PriceChf = b.PriceChf,
        PurchaseDate = b.PurchaseDate,
        Bin = b.Bin,
        Status = b.Status,
        Notes = b.Notes,
        ReadyToDrink = b.ReadyToDrink,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
