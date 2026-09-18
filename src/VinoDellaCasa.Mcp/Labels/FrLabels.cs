using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Mcp.Labels;

/// <summary>French UI labels and color tokens (read-only, product copy).</summary>
public static class FrLabels
{
    public const string Ready = "Prêt";
    public const string Wait = "À attendre";
    public const string NoVintage = "Sans millésime";

    public static string ColorLabel(Color color) => color switch
    {
        Color.Red => "Rouge",
        Color.White => "Blanc",
        Color.Rose => "Rosé",
        Color.Sparkling => "Effervescent",
        Color.Other => "Autre",
        _ => "Autre"
    };

    public static string ColorHex(Color color) => color switch
    {
        Color.Red => "#6B1E2A",
        Color.White => "#C9A227",
        Color.Rose => "#E8A0A8",
        Color.Sparkling => "#E8D5A3",
        Color.Other => "#6B635C",
        _ => "#6B635C"
    };

    public const string ReadyHex = "#2F6B4F";
    public const string WaitHex = "#8A6A2F";

    public const string EmptyNoneReady =
        "Rien de prêt ce soir — Les bouteilles en cave ne sont pas encore dans leur fenêtre, ou la cave est vide.";

    public const string EmptyCellar =
        "Cave vide — Ajoutez des bouteilles pour obtenir des suggestions.";

    public const string EmptyFilter =
        "Aucun résultat — Élargissez la couleur ou la région.";
}
