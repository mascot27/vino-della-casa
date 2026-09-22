using VinoDellaCasa.Domain.Pairing;

namespace VinoDellaCasa.Domain.Entities;

/// <summary>
/// Optional structured taste snapshot for pairing (seed Bourgogne / private catalog).
/// </summary>
public sealed class WineTaste
{
    public TasteBody? Body { get; init; }
    public TasteTannin? Tannin { get; init; }
    public TasteAcidity? Acidity { get; init; }
    public TasteOak? Oak { get; init; }
    public IReadOnlyList<string> PairingHints { get; init; } = [];
}
