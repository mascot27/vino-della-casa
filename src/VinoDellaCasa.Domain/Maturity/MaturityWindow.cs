namespace VinoDellaCasa.Domain.Maturity;

/// <summary>
/// Inclusive peak drinking window as ages in whole calendar years since vintage.
/// </summary>
public readonly record struct MaturityWindow(int PeakOpenAge, int PeakCloseAge);
