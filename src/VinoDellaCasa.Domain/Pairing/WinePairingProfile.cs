using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Pairing;

/// <summary>
/// Input snapshot for <see cref="PairingEngine"/> — catalog or cellar bottle.
/// </summary>
public sealed class WinePairingProfile
{
    public Guid? Id { get; init; }
    public string? Name { get; init; }
    public Color Color { get; init; }
    public string? Region { get; init; }
    public string? Style { get; init; }
    public string? Blend { get; init; }
    public bool ReadyToDrink { get; init; }

    public TasteBody Body { get; init; } = TasteBody.Medium;
    public TasteTannin Tannin { get; init; } = TasteTannin.Medium;
    public TasteAcidity Acidity { get; init; } = TasteAcidity.Medium;
    public TasteOak Oak { get; init; } = TasteOak.Subtle;

    public IReadOnlyList<string> StyleTags { get; init; } = [];
    public IReadOnlyList<string> PairingHints { get; init; } = [];
}
