using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>
/// SampleBordeaux Bordeaux (8) + CH whites (2). Prices only when present on the SampleBordeaux PDF.
/// ReadyToDrink is computed via <see cref="MaturityRules"/> for the given asOfYear.
/// </summary>
public static class M1SeedData
{
    public static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Bottle> CreateM1Seeds(int asOfYear = 2026)
    {
        var specs = new (Guid Id, string Name, string Producer, string Region, string Country,
            Color Color, string Varietal, int Vintage, int Quantity, decimal? PriceChf,
            string Bin, string Notes, MaturityStyle Style)[]
        {
            (Guid.Parse("a1000001-0000-4000-8000-000000000001"),
                "Domaine de la Solitude", "Domaine de la Solitude", "Pessac-Léognan", "France",
                Color.Red, "CS, Merlot, Petit Verdot", 2018, 2, 25.00m, "A1",
                "RP 92 · fenêtre indicative 2024–2028 · MaturityStyle=PessacRougeAccessible",
                MaturityStyle.PessacRougeAccessible),

            (Guid.Parse("a1000001-0000-4000-8000-000000000002"),
                "Château La Fleur de Gay", "Château La Fleur de Gay", "Pomerol", "France",
                Color.Red, "Merlot", 2014, 1, 93.00m, "B2",
                "RP 92 · musculaire, garde · MaturityStyle=PomerolMerlot",
                MaturityStyle.PomerolMerlot),

            (Guid.Parse("a1000001-0000-4000-8000-000000000003"),
                "Château Larcis Ducasse", "Château Larcis Ducasse", "Saint-Émilion", "France",
                Color.Red, "Merlot 90 / Cabernet Franc 10", 2020, 1, 93.00m, "B1",
                "1er Grand Cru Classé 2022 · RP 96 · Hachette ♥ · 2027–2040 · MaturityStyle=StEmilionPremierGcc",
                MaturityStyle.StEmilionPremierGcc),

            (Guid.Parse("a1000001-0000-4000-8000-000000000004"),
                "Pagode de Cos", "Château Cos d’Estournel", "Saint-Estèphe", "France",
                Color.Red, "Cabernet Sauvignon dominant", 2019, 2, 51.00m, "A2",
                "2e vin Cos d’Estournel · RP 92–94 · MaturityStyle=MedocSecondVin",
                MaturityStyle.MedocSecondVin),

            (Guid.Parse("a1000001-0000-4000-8000-000000000005"),
                "L’Esprit de Chevalier", "Domaine de Chevalier", "Pessac-Léognan", "France",
                Color.Red, "CS 55 / Merlot 40 / PV 5", 2019, 1, null, "A3",
                "2e vin Domaine de Chevalier · RP 90 · dès ~2025 · prix absent du PDF · MaturityStyle=PessacRougeSecondVin",
                MaturityStyle.PessacRougeSecondVin),

            (Guid.Parse("a1000001-0000-4000-8000-000000000006"),
                "Château Pape Clément", "Château Pape Clément", "Pessac-Léognan", "France",
                Color.Red, "CS 50 / Merlot 50", 2019, 1, 98.00m, "C1",
                "Cru classé Graves · RP 94 · Hachette ♥ · 2027–2034 · MaturityStyle=PessacRougeClasse",
                MaturityStyle.PessacRougeClasse),

            (Guid.Parse("a1000001-0000-4000-8000-000000000007"),
                "Château Troplong Mondot", "Château Troplong Mondot", "Saint-Émilion", "France",
                Color.Red, "Merlot, Cabernet Franc, Cabernet Sauvignon", 2014, 1, 105.00m, "B3",
                "1er Grand Cru Classé · RP 94 · 2022–2030 · MaturityStyle=StEmilionPremierGcc",
                MaturityStyle.StEmilionPremierGcc),

            (Guid.Parse("a1000001-0000-4000-8000-000000000008"),
                "Domaine de Chevalier", "Domaine de Chevalier", "Pessac-Léognan", "France",
                Color.Red, "CS 65 / Merlot 27 / PV 5 / CF 3", 2020, 1, 80.00m, "C2",
                "Cru classé Graves · RP 95+ · garde · MaturityStyle=PessacRougeClasse",
                MaturityStyle.PessacRougeClasse),

            (Guid.Parse("a1000001-0000-4000-8000-000000000009"),
                "Clos des Abbayes", "Domaine du Daley", "Lavaux", "Suisse",
                Color.White, "Chasselas", 2023, 2, null, "D1",
                "Complément CH (hors PDF SampleBordeaux) · filtre Color/Country · MaturityStyle=BlancSecSuisse · prix non inventé",
                MaturityStyle.BlancSecSuisse),

            (Guid.Parse("a1000001-0000-4000-8000-000000000010"),
                "Petite Arvine", "Cave du Rhodan", "Valais", "Suisse",
                Color.White, "Petite Arvine", 2022, 1, null, "D2",
                "Complément CH (hors PDF SampleBordeaux) · filtre Color/Varietal · MaturityStyle=BlancSecSuisse · prix non inventé",
                MaturityStyle.BlancSecSuisse),
        };

        var bottles = new List<Bottle>(specs.Length);
        foreach (var s in specs)
        {
            var bottle = new Bottle
            {
                Id = s.Id,
                Name = s.Name,
                Producer = s.Producer,
                Region = s.Region,
                Country = s.Country,
                Color = s.Color,
                Varietal = s.Varietal,
                Vintage = s.Vintage,
                Quantity = s.Quantity,
                PriceChf = s.PriceChf,
                PurchaseDate = null,
                Bin = s.Bin,
                Status = BottleStatus.InStock,
                Notes = s.Notes,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            };
            MaturityRules.ApplyReadyToDrink(bottle, asOfYear, s.Style);
            bottle.UpdatedAt = SeedTimestamp;
            bottles.Add(bottle);
        }

        return bottles;
    }
}
