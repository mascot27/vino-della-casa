using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Domain.Pairing;

/// <summary>
/// Deterministic food ↔ wine pairing (Docs/domain/PAIRING-RULES.md). Offline, no LLM.
/// </summary>
public static class PairingEngine
{
    public const int DefaultTopN = 5;

    /// <summary>Canonical FR dish labels used by tables + UI.</summary>
    public static IReadOnlyList<string> KnownDishes { get; } =
    [
        "apéritif",
        "volaille rôtie",
        "volaille crème",
        "charcuterie",
        "champignons",
        "pizza",
        "agneau",
        "agneau rôti",
        "bœuf grillé",
        "bœuf mijoté",
        "côte de bœuf",
        "pâtes viande",
        "comté",
        "Comté jeune",
        "fromages affinés",
        "magret",
        "canard",
        "gibier",
        "chocolat noir",
        "huîtres",
        "fruits de mer",
        "chèvre frais",
        "veau",
        "homard",
        "ris de veau",
        "poularde",
        "cuisine asiatique douce",
        "fromage bleu doux",
        "bleu",
        "grillades légères",
        "salades",
        "cuisine méditerranéenne",
        "sushis",
        "fritures légères",
        "desserts fruités",
        "foie gras",
        "grillades",
        "ratatouille"
    ];

    public static IReadOnlyList<DishSuggestion> SuggestDishes(
        WinePairingProfile profile,
        int topN = DefaultTopN)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (topN < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(topN), topN, "topN must be >= 1.");
        }

        var scores = new Dictionary<string, (int Score, string Why)>(StringComparer.OrdinalIgnoreCase);
        void Boost(string dish, int delta, string why)
        {
            if (delta == 0)
            {
                return;
            }

            if (scores.TryGetValue(dish, out var cur))
            {
                var next = Math.Clamp(cur.Score + delta, 0, 100);
                // Keep the why from the strongest contribution.
                scores[dish] = next >= cur.Score
                    ? (next, delta > 0 ? why : cur.Why)
                    : (next, cur.Why);
            }
            else if (delta > 0)
            {
                scores[dish] = (Math.Clamp(delta, 0, 100), why);
            }
        }

        ApplyColorBase(profile, Boost);
        ApplyOakBonus(profile, Boost);
        ApplyRegionBonus(profile, Boost);
        ApplyHintOverlap(profile, Boost);
        ApplyReadinessPenalty(profile, Boost);

        return scores
            .Select(kv => new DishSuggestion(CanonicalDish(kv.Key), kv.Value.Score, kv.Value.Why))
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Dish, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .ToList();
    }

    public static IReadOnlyList<BottlePairingMatch> SuggestBottles(
        string dish,
        IEnumerable<WinePairingProfile> candidates,
        int topN = DefaultTopN,
        bool catalogFirst = true)
    {
        ArgumentNullException.ThrowIfNull(dish);
        ArgumentNullException.ThrowIfNull(candidates);
        if (topN < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(topN), topN, "topN must be >= 1.");
        }

        var needle = Normalize(dish);
        if (string.IsNullOrEmpty(needle))
        {
            return [];
        }

        var matches = new List<BottlePairingMatch>();
        foreach (var profile in candidates)
        {
            var dishes = SuggestDishes(profile, topN: KnownDishes.Count);
            var hit = dishes.FirstOrDefault(d =>
                Normalize(d.Dish) == needle
                || Normalize(d.Dish).Contains(needle, StringComparison.Ordinal)
                || needle.Contains(Normalize(d.Dish), StringComparison.Ordinal));

            if (hit is null)
            {
                continue;
            }

            var score = hit.Score;
            if (!catalogFirst && !profile.ReadyToDrink)
            {
                score = Math.Max(0, score - 8);
            }

            matches.Add(new BottlePairingMatch(profile, score, hit.Why));
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenByDescending(m => m.Profile.ReadyToDrink)
            .ThenBy(m => m.Profile.Name, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .ToList();
    }

    /// <summary>Score one dish against a profile (0 if no affinity).</summary>
    public static int ScoreDish(WinePairingProfile profile, string dish)
    {
        var list = SuggestDishes(profile, topN: KnownDishes.Count);
        var needle = Normalize(dish);
        return list
            .Where(d => Normalize(d.Dish) == needle
                        || Normalize(d.Dish).Contains(needle, StringComparison.Ordinal)
                        || needle.Contains(Normalize(d.Dish), StringComparison.Ordinal))
            .Select(d => d.Score)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static void ApplyColorBase(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        switch (profile.Color)
        {
            case Color.Red:
                ApplyRedBase(profile, boost);
                break;
            case Color.White:
                ApplyWhiteBase(profile, boost);
                break;
            case Color.Rose:
                foreach (var (dish, score) in new (string, int)[]
                         {
                             ("apéritif", 78),
                             ("grillades légères", 82),
                             ("salades", 80),
                             ("cuisine méditerranéenne", 78),
                             ("pizza", 74)
                         })
                {
                    boost(dish, score, "Rosé frais — table estivale");
                }

                break;
            case Color.Sparkling:
                foreach (var (dish, score) in new (string, int)[]
                         {
                             ("apéritif", 86),
                             ("fruits de mer", 80),
                             ("sushis", 78),
                             ("fritures légères", 74),
                             ("desserts fruités", 70)
                         })
                {
                    boost(dish, score, "Bulles — tension et apéritif");
                }

                break;
            default:
                boost("apéritif", 60, "Profil ouvert — apéritif");
                break;
        }
    }

    private static void ApplyRedBase(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        if (profile.Body == TasteBody.Light && profile.Tannin == TasteTannin.Soft)
        {
            boost("volaille rôtie", 82, "Rouge léger, tanins souples");
            boost("charcuterie", 80, "Rouge léger, tanins souples");
            boost("champignons", 76, "Rouge léger, tanins souples");
            boost("pizza", 72, "Rouge léger, tanins souples");
            return;
        }

        if (profile.Body == TasteBody.Full && profile.Tannin == TasteTannin.Firm)
        {
            boost("bœuf mijoté", 88, "Structure tannique + corps plein");
            boost("gibier", 86, "Structure tannique + corps plein");
            boost("côte de bœuf", 84, "Structure tannique + corps plein");
            boost("fromages affinés", 78, "Structure tannique + corps plein");
            boost("agneau", 80, "Structure tannique + corps plein");
            boost("agneau rôti", 82, "Structure tannique + corps plein");
            return;
        }

        // medium (default) and other red combinations
        boost("agneau", 82, "Rouge de corps moyen — viandes");
        boost("agneau rôti", 80, "Rouge de corps moyen — viandes");
        boost("bœuf grillé", 80, "Rouge de corps moyen — viandes");
        boost("pâtes viande", 76, "Rouge de corps moyen — viandes");
        boost("comté", 74, "Rouge de corps moyen — fromage");

        if (profile.Body == TasteBody.Full)
        {
            boost("bœuf mijoté", 10, "Corps plein");
            boost("gibier", 8, "Corps plein");
        }

        if (profile.Tannin == TasteTannin.Soft)
        {
            boost("volaille rôtie", 8, "Tanins souples");
            boost("charcuterie", 6, "Tanins souples");
        }
    }

    private static void ApplyWhiteBase(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        var aromatic = HasAnyTag(profile, "aromatic", "off-dry", "liquoreux", "moelleux", "gewurz", "riesling");

        if (aromatic || ContainsAny(profile.Region, "Sauternes", "Barsac")
                      || HasAnyTag(profile, "sauternes", "liquoreux"))
        {
            boost("foie gras", 88, "Blanc liquoreux / aromatique");
            boost("bleu", 82, "Blanc liquoreux / aromatique");
            boost("fromage bleu doux", 80, "Blanc liquoreux / aromatique");
            boost("cuisine asiatique douce", 74, "Blanc aromatique");
            return;
        }

        if (profile.Acidity == TasteAcidity.High
            && (profile.Body == TasteBody.Light
                || HasAnyTag(profile, "chablis-style", "mineral", "flint", "iodé", "iode")))
        {
            boost("huîtres", 90, "Acidité vive + profil Chablis");
            boost("fruits de mer", 86, "Acidité vive + profil Chablis");
            boost("chèvre frais", 80, "Acidité vive");
            return;
        }

        if (profile.Acidity == TasteAcidity.High && profile.Body == TasteBody.Light)
        {
            boost("huîtres", 88, "Acidité haute, corps léger");
            boost("fruits de mer", 84, "Acidité haute, corps léger");
            boost("chèvre frais", 78, "Acidité haute, corps léger");
            return;
        }

        if (profile.Body == TasteBody.Full && profile.Oak == TasteOak.Marked)
        {
            boost("homard", 88, "Blanc ample, bois marqué");
            boost("ris de veau", 84, "Blanc ample, bois marqué");
            boost("poularde", 82, "Blanc ample, bois marqué");
            return;
        }

        // medium acid + subtle oak (default blanc)
        boost("volaille crème", 80, "Blanc équilibré");
        boost("veau", 78, "Blanc équilibré");
        boost("Comté jeune", 76, "Blanc équilibré");
        boost("fruits de mer", 72, "Blanc équilibré");

        if (profile.Acidity == TasteAcidity.High)
        {
            boost("huîtres", 12, "Acidité haute");
            boost("fruits de mer", 10, "Acidité haute");
            boost("chèvre frais", 8, "Acidité haute");
        }

        if (profile.Body == TasteBody.Full)
        {
            boost("homard", 10, "Corps plein");
            boost("poularde", 8, "Corps plein");
        }
    }

    private static void ApplyOakBonus(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        if (profile.Color != Color.Red || profile.Oak != TasteOak.Marked)
        {
            return;
        }

        boost("bœuf grillé", 10, "Élevage marqué — grillades");
        boost("magret", 12, "Élevage marqué — magret");
        boost("chocolat noir", 5, "Élevage marqué — dessert rare");
    }

    private static void ApplyRegionBonus(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        var region = profile.Region ?? string.Empty;
        var hay = $"{region} {profile.Style} {profile.Blend} {string.Join(' ', profile.StyleTags)}";

        if (ContainsAny(hay, "Chablis", "iodé", "iode", "flint", "chablis-style", "mineral"))
        {
            boost("huîtres", 15, "Affinage Chablis / iodé");
            boost("fruits de mer", 12, "Affinage Chablis / iodé");
        }

        if (ContainsAny(hay, "Beaujolais", "gamay", "easy"))
        {
            boost("charcuterie", 12, "Affinage Beaujolais / gamay");
            boost("volaille rôtie", 10, "Affinage Beaujolais / gamay");
        }

        if (ContainsAny(hay, "Saint-Émilion", "Saint-Emilion", "Pomerol", "merlot"))
        {
            boost("agneau", 12, "Affinage rive droite / merlot");
            boost("agneau rôti", 10, "Affinage rive droite / merlot");
            boost("canard", 12, "Affinage rive droite / merlot");
        }

        if (ContainsAny(hay, "Médoc", "Medoc", "Pauillac", "Saint-Estèphe", "Saint-Julien",
                "Haut-Médoc", "Margaux", "cabernet", "firm"))
        {
            boost("agneau", 10, "Affinage Médoc / cabernet");
            boost("agneau rôti", 8, "Affinage Médoc / cabernet");
            boost("bœuf grillé", 12, "Affinage Médoc / cabernet");
        }

        if (ContainsAny(hay, "Sauternes", "Barsac", "liquoreux"))
        {
            boost("foie gras", 15, "Affinage liquoreux");
            boost("bleu", 12, "Affinage liquoreux");
        }

        if (ContainsAny(hay, "Rhône", "grenache", "Châteauneuf", "Gigondas"))
        {
            boost("grillades", 12, "Affinage Rhône sud");
            boost("ratatouille", 10, "Affinage Rhône sud");
        }

        if (ContainsAny(hay, "Provence") || profile.Color == Color.Rose && ContainsAny(hay, "Provence"))
        {
            boost("salades", 10, "Affinage Provence");
            boost("grillades", 8, "Affinage Provence");
            boost("grillades légères", 10, "Affinage Provence");
        }

        if (ContainsAny(hay, "Pessac", "Graves"))
        {
            boost("volaille rôtie", 8, "Affinage Pessac / Graves");
            boost("champignons", 8, "Affinage Pessac / Graves");
        }
    }

    private static void ApplyHintOverlap(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        foreach (var raw in profile.PairingHints)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var dish = MapHintToDish(raw);
            if (dish is null)
            {
                // Free hint: try as dish label itself.
                dish = CanonicalDish(raw.Trim());
            }

            boost(dish, 10, "Hint accords catalogue");
        }
    }

    private static void ApplyReadinessPenalty(
        WinePairingProfile profile,
        Action<string, int, string> boost)
    {
        if (profile.ReadyToDrink || profile.Color != Color.Red)
        {
            return;
        }

        if (profile.Body == TasteBody.Full || profile.Tannin == TasteTannin.Firm)
        {
            // Soft down-rank heavy dishes when too young.
            boost("bœuf mijoté", -8, "Encore jeune — structure à attendre");
            boost("gibier", -8, "Encore jeune — structure à attendre");
            boost("côte de bœuf", -6, "Encore jeune — structure à attendre");
        }
    }

    private static string? MapHintToDish(string hint)
    {
        var n = Normalize(hint);
        return n switch
        {
            "oysters" or "huitres" or "huîtres" => "huîtres",
            "lobster" or "homard" => "homard",
            "comte" or "comté" => "comté",
            "comte jeune" or "comté jeune" => "Comté jeune",
            "game" or "gibier" => "gibier",
            "mushroom risotto" or "champignons" or "mushrooms" => "champignons",
            "duck" or "canard" or "magret" => n.Contains("magret", StringComparison.Ordinal) ? "magret" : "canard",
            "lamb" or "agneau" or "agneau roti" or "agneau rôti" => "agneau",
            "beef" or "steak" or "boeuf grille" or "bœuf grillé" => "bœuf grillé",
            "foie gras" => "foie gras",
            "sushi" or "sushis" => "sushis",
            "aperitif" or "apéritif" => "apéritif",
            "charcuterie" => "charcuterie",
            "pizza" => "pizza",
            "poultry" or "volaille" or "volaille rotie" or "volaille rôtie" => "volaille rôtie",
            "seafood" or "fruits de mer" => "fruits de mer",
            "goat" or "chevre" or "chèvre frais" => "chèvre frais",
            "blue cheese" or "bleu" => "bleu",
            _ => null
        };
    }

    private static string CanonicalDish(string dish)
    {
        foreach (var known in KnownDishes)
        {
            if (string.Equals(known, dish, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return dish.Trim();
    }

    private static bool HasAnyTag(WinePairingProfile profile, params string[] needles)
    {
        foreach (var tag in profile.StyleTags)
        {
            if (ContainsAny(tag, needles))
            {
                return true;
            }
        }

        return ContainsAny(profile.Style, needles);
    }

    private static bool ContainsAny(string? source, params string[] needles)
    {
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }

        foreach (var n in needles)
        {
            if (source.Contains(n, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var s = value.Trim().ToLowerInvariant();
        return s
            .Replace('é', 'e')
            .Replace('è', 'e')
            .Replace('ê', 'e')
            .Replace('à', 'a')
            .Replace('â', 'a')
            .Replace('ô', 'o')
            .Replace('î', 'i')
            .Replace('ï', 'i')
            .Replace('ü', 'u')
            .Replace('ù', 'u')
            .Replace('ç', 'c')
            .Replace("œ", "oe", StringComparison.Ordinal);
    }
}
