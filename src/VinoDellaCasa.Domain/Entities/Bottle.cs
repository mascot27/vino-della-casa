using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Entities;

/// <summary>
/// Denormalized cellar bottle (MVP). See Docs/SPEC.md.
/// </summary>
public sealed class Bottle
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }
    public string? Producer { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }

    public Color Color { get; set; }
    public string? Varietal { get; set; }
    public int? Vintage { get; set; }

    public int Quantity { get; set; }
    public decimal? PriceChf { get; set; }
    public DateOnly? PurchaseDate { get; set; }

    public string? Bin { get; set; }
    public BottleStatus Status { get; set; } = BottleStatus.InStock;

    public string? Notes { get; set; }
    public bool ReadyToDrink { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
