namespace VinoDellaCasa.Domain.Suggestions;

/// <summary>
/// Free-string autocomplete suggestions (not closed enums). See Docs/domain/M1-enums-draft.md.
/// </summary>
public static class FieldSuggestions
{
    public static IReadOnlyList<string> Varietal { get; } =
    [
        // Rouges prioritaires
        "Cabernet Sauvignon",
        "Merlot",
        "Cabernet Franc",
        "Petit Verdot",
        "Pinot Noir",
        "Gamay",
        "Syrah",
        "Cornalin",
        "Humagne Rouge",
        // Rouges secondaires
        "Nebbiolo",
        "Sangiovese",
        "Tempranillo",
        "Malbec",
        "Grenache",
        // Blancs
        "Chardonnay",
        "Sauvignon Blanc",
        "Sémillon",
        "Riesling",
        "Chasselas",
        "Petite Arvine",
        "Chenin Blanc",
        "Pinot Gris",
        "Gewurztraminer",
        "Viognier",
        "Grüner Veltliner"
    ];

    public static IReadOnlyList<string> Region { get; } =
    [
        // Bordeaux (UX order)
        "Pessac-Léognan",
        "Saint-Émilion",
        "Pomerol",
        "Saint-Estèphe",
        "Pauillac",
        "Margaux",
        "Saint-Julien",
        "Médoc",
        "Haut-Médoc",
        "Graves",
        "Sauternes",
        "Bordeaux",
        // Suisse
        "Valais",
        "Vaud",
        "Lavaux",
        "Tessin",
        "Genève",
        "Neuchâtel",
        "Trois Lacs",
        "Grisons",
        // Secondaire
        "Bourgogne",
        "Champagne",
        "Loire",
        "Rhône",
        "Alsace",
        "Beaujolais",
        "Toscane",
        "Piémont",
        "Rioja"
    ];

    public static IReadOnlyList<string> Country { get; } =
    [
        "France",
        "Suisse",
        "Italie",
        "Espagne",
        "Allemagne",
        "Autriche",
        "Portugal",
        "États-Unis",
        "Chili",
        "Argentine",
        "Afrique du Sud",
        "Australie",
        "Nouvelle-Zélande"
    ];

    public static IEnumerable<string> Filter(IEnumerable<string> source, string? query, int take = 12)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return source.Take(take);
        }

        var q = query.Trim();
        return source
            .Where(s => s.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.StartsWith(q, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Take(take);
    }
}
