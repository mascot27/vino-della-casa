using VinoDellaCasa.Domain.Entities;

namespace VinoDellaCasa.Application.Abstractions;

/// <summary>
/// Async CRUD for the personal cellar. Browser path: IndexedDB-backed implementation.
/// Host/tests path: in-memory or EF Core + SQLite.
/// </summary>
public interface ICellarStore
{
    Task<IReadOnlyList<Bottle>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Bottle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Bottle> AddAsync(Bottle bottle, CancellationToken cancellationToken = default);

    Task<Bottle> UpdateAsync(Bottle bottle, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
