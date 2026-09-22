namespace VinoDellaCasa.Domain.Pairing;

/// <summary>Scored dish tag for bottle → mets.</summary>
public sealed record DishSuggestion(string Dish, int Score, string Why);

/// <summary>Scored bottle for dish → bouteilles.</summary>
public sealed record BottlePairingMatch(
    WinePairingProfile Profile,
    int Score,
    string Why);
