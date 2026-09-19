using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Entities;

/// <summary>
/// Catalogue fiche (anonymous sommelier sheet). Cellar ownership is local qty only — not stored here.
/// </summary>
public sealed class CatalogEntry
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public string? Producer { get; set; }

    /// <summary>Appellation / région (e.g. Pessac-Léognan).</summary>
    public required string Appellation { get; set; }

    public string Country { get; set; } = "France";
    public Color Color { get; set; }
    public int? Vintage { get; set; }

    /// <summary>Local path under wwwroot, e.g. img/catalog/estate-chateau.jpg.</summary>
    public required string ImagePath { get; set; }

    /// <summary>Short style line (italic on fiche), e.g. « fruité · structure souple ».</summary>
    public required string Style { get; set; }

    /// <summary>1–2 anonymous métier sentences for the Pourquoi block.</summary>
    public required string Pourquoi { get; set; }

    /// <summary>Assemblage / cépage(s).</summary>
    public string? Blend { get; set; }

    public string? Classification { get; set; }

    /// <summary>Legacy generic star score; prefer structured critic scores below.</summary>
    public int? Stars { get; set; }

    public string? DrinkWindow { get; set; }

    /// <summary>Anonymous pairing hint (facts only, no critic prose).</summary>
    public string? Pairing { get; set; }

    /// <summary>Robert Parker score when known as a number (no prose).</summary>
    public int? ScoreRp { get; set; }

    /// <summary>James Suckling score when known as a number (no prose).</summary>
    public int? ScoreJs { get; set; }

    /// <summary>Hachette mark when known (e.g. « ♥ », « ** »), facts only.</summary>
    public string? ScoreHachette { get; set; }

    /// <summary>Indicative price range in CHF when known (e.g. « 40–55 »).</summary>
    public string? PriceRangeChf { get; set; }

    /// <summary>Computed for asOfYear when seed is built; UI may refresh via MaturityRules.</summary>
    public bool ReadyToDrink { get; set; }
}
