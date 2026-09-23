using VinoDellaCasa.Domain.Entities;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// Scrubbed public <c>blendsByVintage</c> facts for demo catalogue estates
/// (wine-knowledge export, grape/% only — no personal/agent meta).
/// </summary>
public static class CatalogDemoBlends
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<BlendByVintage>> ByName =
        new Dictionary<string, IReadOnlyList<BlendByVintage>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Château Angélus"] =
            [
                new BlendByVintage
                {
                    Vintage = 2008,
                    Components =
                    [
                        new BlendComponent { Grape = "Merlot", Percent = 53 },
                        new BlendComponent { Grape = "Cabernet Franc", Percent = 47 }
                    ]
                }
            ],
            ["Château Cheval Blanc"] =
            [
                new BlendByVintage
                {
                    Vintage = 2022,
                    Components =
                    [
                        new BlendComponent { Grape = "Merlot", Percent = 53 },
                        new BlendComponent { Grape = "Cabernet Franc", Percent = 46 },
                        new BlendComponent { Grape = "Cabernet Sauvignon", Percent = 1 }
                    ]
                },
                new BlendByVintage
                {
                    Vintage = 2021,
                    Components =
                    [
                        new BlendComponent { Grape = "Cabernet Franc", Percent = 52 },
                        new BlendComponent { Grape = "Merlot", Percent = 48 }
                    ]
                },
                new BlendByVintage
                {
                    Vintage = 2020,
                    Components =
                    [
                        new BlendComponent { Grape = "Merlot", Percent = 65 },
                        new BlendComponent { Grape = "Cabernet Franc", Percent = 30 },
                        new BlendComponent { Grape = "Cabernet Sauvignon", Percent = 5 }
                    ]
                }
            ],
            ["Château Climens"] =
            [
                new BlendByVintage
                {
                    Vintage = 2016,
                    Components = [new BlendComponent { Grape = "Sémillon", Percent = 100 }]
                }
            ],
            ["Château Léoville Barton"] =
            [
                new BlendByVintage
                {
                    Vintage = 2022,
                    Components =
                    [
                        new BlendComponent { Grape = "Cabernet Sauvignon", Percent = 83 },
                        new BlendComponent { Grape = "Merlot", Percent = 17 }
                    ]
                },
                new BlendByVintage
                {
                    Vintage = 2010,
                    Components =
                    [
                        new BlendComponent { Grape = "Cabernet Sauvignon", Percent = 77 },
                        new BlendComponent { Grape = "Merlot", Percent = 21 },
                        new BlendComponent { Grape = "Cabernet Franc", Percent = 2 }
                    ]
                }
            ],
            ["Château Suduiraut"] =
            [
                new BlendByVintage
                {
                    Vintage = 2020,
                    Components = [new BlendComponent { Grape = "Sémillon", Percent = 100 }]
                }
            ]
        };

    /// <summary>Scrubbed blends for a demo estate name, or empty when none.</summary>
    public static IReadOnlyList<BlendByVintage> ForName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || !ByName.TryGetValue(name.Trim(), out var blends))
        {
            return [];
        }

        return CatalogAssemblage.MapFromSeed(blends);
    }
}
