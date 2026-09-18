using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Web;

/// <summary>French UI labels for domain enums (domain stays English).</summary>
public static class UiLabels
{
    public static string Color(Color c) => c switch
    {
        Domain.Enums.Color.Red => "Rouge",
        Domain.Enums.Color.White => "Blanc",
        Domain.Enums.Color.Rose => "Rosé",
        Domain.Enums.Color.Sparkling => "Effervescent",
        Domain.Enums.Color.Other => "Autre",
        _ => c.ToString()
    };

    public static string ColorCss(Color c) => c switch
    {
        Domain.Enums.Color.Red => "rouge",
        Domain.Enums.Color.White => "blanc",
        Domain.Enums.Color.Rose => "rose",
        Domain.Enums.Color.Sparkling => "effervescent",
        _ => "autre"
    };

    public static string Status(BottleStatus s) => s switch
    {
        BottleStatus.InStock => "En cave",
        BottleStatus.Drinking => "En cours",
        BottleStatus.Finished => "Fini",
        _ => s.ToString()
    };

    /// <summary>Maturity badge: Prêt / À attendre / Sans millésime.</summary>
    public static string MaturityBadge(BottleReadyState state) => state switch
    {
        BottleReadyState.Ready => "Prêt",
        BottleReadyState.Wait => "À attendre",
        _ => "Sans millésime"
    };

    public static BottleReadyState MaturityState(bool readyToDrink, int? vintage)
    {
        if (vintage is null)
        {
            return BottleReadyState.NoVintage;
        }

        return readyToDrink ? BottleReadyState.Ready : BottleReadyState.Wait;
    }

    public static string MaturityCss(BottleReadyState state) => state switch
    {
        BottleReadyState.Ready => "pret",
        BottleReadyState.Wait => "attendre",
        _ => "sans-millesime"
    };

    public static string VintageLine(int? vintage, int asOfYear)
    {
        if (vintage is null)
        {
            return "Sans millésime";
        }

        var age = asOfYear - vintage.Value;
        var ageLabel = age <= 1 ? (age == 1 ? "1 an" : "0 an") : $"{age} ans";
        if (age < 0)
        {
            ageLabel = $"{vintage}";
            return vintage.Value.ToString();
        }

        return $"{vintage} · {ageLabel}";
    }
}

public enum BottleReadyState
{
    Ready,
    Wait,
    NoVintage
}
