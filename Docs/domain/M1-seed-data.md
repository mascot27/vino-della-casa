# M1 — Seed data (draft métier)

> 10 bouteilles · Profil sample Bordeaux + 2 compléments CH (Color / filtres)  
> Prix CHF **uniquement** ceux du PDF sample Bordeaux ; sinon `null`  
> `asOfYear` référence seeds : **2026** · ReadyToDrink selon `M1-maturity-rules.md`  
> Date draft : 2026-09-18 (Europe/Zurich)

---

## 1. Tableau résumé

| # | Name | Region | Vintage | PriceChf | Color | Ready | Style maturité |
|---|------|--------|---------|----------|-------|-------|----------------|
| 1 | Domaine de la Solitude | Pessac-Léognan | 2018 | 25 | Red | true | PessacRougeAccessible |
| 2 | Château La Fleur de Gay | Pomerol | 2014 | 93 | Red | true | PomerolMerlot |
| 3 | Château Larcis Ducasse | Saint-Émilion | 2020 | 93 | Red | false | StEmilionPremierGcc |
| 4 | Pagode de Cos | Saint-Estèphe | 2019 | 51 | Red | true | MedocSecondVin |
| 5 | L’Esprit de Chevalier | Pessac-Léognan | 2019 | null | Red | true | PessacRougeSecondVin |
| 6 | Château Pape Clément | Pessac-Léognan | 2019 | 98 | Red | false | PessacRougeClasse |
| 7 | Château Troplong Mondot | Saint-Émilion | 2014 | 105 | Red | true | StEmilionPremierGcc |
| 8 | Domaine de Chevalier | Pessac-Léognan | 2020 | 80 | Red | false | PessacRougeClasse |
| 9 | Domaine du Daley — Clos des Abbayes | Lavaux | 2023 | null | White | true | BlancSecSuisse |
| 10 | Cave du Rhodan — Petite Arvine | Valais | 2022 | null | White | true | BlancSecSuisse |

Compléments 9–10 : **pas** dans le PDF sample Bordeaux — prix `null` (exactitude). Producteurs CH réalistes pour tester filtres Color/Country/Region ; remplaçables sans casser les règles.

---

## 2. Payload JSON-like (prêt C# seed)

Guids stables pour tests ; timestamps ISO UTC. `Status = InStock`, `Quantity = 1` sauf indication.

```json
[
  {
    "Id": "a1000001-0000-4000-8000-000000000001",
    "Name": "Domaine de la Solitude",
    "Producer": "Domaine de la Solitude",
    "Region": "Pessac-Léognan",
    "Country": "France",
    "Color": "Red",
    "Varietal": "CS, Merlot, Petit Verdot",
    "Vintage": 2018,
    "Quantity": 2,
    "PriceChf": 25.00,
    "PurchaseDate": null,
    "Bin": "A1",
    "Status": "InStock",
    "Notes": "RP 92 · fenêtre indicative 2024–2028 · MaturityStyle=PessacRougeAccessible",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000002",
    "Name": "Château La Fleur de Gay",
    "Producer": "Château La Fleur de Gay",
    "Region": "Pomerol",
    "Country": "France",
    "Color": "Red",
    "Varietal": "Merlot",
    "Vintage": 2014,
    "Quantity": 1,
    "PriceChf": 93.00,
    "PurchaseDate": null,
    "Bin": "B2",
    "Status": "InStock",
    "Notes": "RP 92 · musculaire, garde · MaturityStyle=PomerolMerlot",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000003",
    "Name": "Château Larcis Ducasse",
    "Producer": "Château Larcis Ducasse",
    "Region": "Saint-Émilion",
    "Country": "France",
    "Color": "Red",
    "Varietal": "Merlot 90 / Cabernet Franc 10",
    "Vintage": 2020,
    "Quantity": 1,
    "PriceChf": 93.00,
    "PurchaseDate": null,
    "Bin": "B1",
    "Status": "InStock",
    "Notes": "1er Grand Cru Classé 2022 · RP 96 · Hachette ♥ · 2027–2040 · MaturityStyle=StEmilionPremierGcc",
    "ReadyToDrink": false,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000004",
    "Name": "Pagode de Cos",
    "Producer": "Château Cos d’Estournel",
    "Region": "Saint-Estèphe",
    "Country": "France",
    "Color": "Red",
    "Varietal": "Cabernet Sauvignon dominant",
    "Vintage": 2019,
    "Quantity": 2,
    "PriceChf": 51.00,
    "PurchaseDate": null,
    "Bin": "A2",
    "Status": "InStock",
    "Notes": "2e vin Cos d’Estournel · RP 92–94 · MaturityStyle=MedocSecondVin",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000005",
    "Name": "L’Esprit de Chevalier",
    "Producer": "Domaine de Chevalier",
    "Region": "Pessac-Léognan",
    "Country": "France",
    "Color": "Red",
    "Varietal": "CS 55 / Merlot 40 / PV 5",
    "Vintage": 2019,
    "Quantity": 1,
    "PriceChf": null,
    "PurchaseDate": null,
    "Bin": "A3",
    "Status": "InStock",
    "Notes": "2e vin Domaine de Chevalier · RP 90 · dès ~2025 · prix absent du PDF · MaturityStyle=PessacRougeSecondVin",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000006",
    "Name": "Château Pape Clément",
    "Producer": "Château Pape Clément",
    "Region": "Pessac-Léognan",
    "Country": "France",
    "Color": "Red",
    "Varietal": "CS 50 / Merlot 50",
    "Vintage": 2019,
    "Quantity": 1,
    "PriceChf": 98.00,
    "PurchaseDate": null,
    "Bin": "C1",
    "Status": "InStock",
    "Notes": "Cru classé Graves · RP 94 · Hachette ♥ · 2027–2034 · MaturityStyle=PessacRougeClasse",
    "ReadyToDrink": false,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000007",
    "Name": "Château Troplong Mondot",
    "Producer": "Château Troplong Mondot",
    "Region": "Saint-Émilion",
    "Country": "France",
    "Color": "Red",
    "Varietal": "Merlot, Cabernet Franc, Cabernet Sauvignon",
    "Vintage": 2014,
    "Quantity": 1,
    "PriceChf": 105.00,
    "PurchaseDate": null,
    "Bin": "B3",
    "Status": "InStock",
    "Notes": "1er Grand Cru Classé · RP 94 · 2022–2030 · MaturityStyle=StEmilionPremierGcc",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000008",
    "Name": "Domaine de Chevalier",
    "Producer": "Domaine de Chevalier",
    "Region": "Pessac-Léognan",
    "Country": "France",
    "Color": "Red",
    "Varietal": "CS 65 / Merlot 27 / PV 5 / CF 3",
    "Vintage": 2020,
    "Quantity": 1,
    "PriceChf": 80.00,
    "PurchaseDate": null,
    "Bin": "C2",
    "Status": "InStock",
    "Notes": "Cru classé Graves · RP 95+ · garde · MaturityStyle=PessacRougeClasse",
    "ReadyToDrink": false,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000009",
    "Name": "Clos des Abbayes",
    "Producer": "Domaine du Daley",
    "Region": "Lavaux",
    "Country": "Suisse",
    "Color": "White",
    "Varietal": "Chasselas",
    "Vintage": 2023,
    "Quantity": 2,
    "PriceChf": null,
    "PurchaseDate": null,
    "Bin": "D1",
    "Status": "InStock",
    "Notes": "Complément CH (hors PDF sample Bordeaux) · filtre Color/Country · MaturityStyle=BlancSecSuisse · prix non inventé",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  },
  {
    "Id": "a1000001-0000-4000-8000-000000000010",
    "Name": "Petite Arvine",
    "Producer": "Cave du Rhodan",
    "Region": "Valais",
    "Country": "Suisse",
    "Color": "White",
    "Varietal": "Petite Arvine",
    "Vintage": 2022,
    "Quantity": 1,
    "PriceChf": null,
    "PurchaseDate": null,
    "Bin": "D2",
    "Status": "InStock",
    "Notes": "Complément CH (hors PDF sample Bordeaux) · filtre Color/Varietal · MaturityStyle=BlancSecSuisse · prix non inventé",
    "ReadyToDrink": true,
    "CreatedAt": "2026-09-18T12:00:00Z",
    "UpdatedAt": "2026-09-18T12:00:00Z"
  }
]
```

---

## 3. Snippet C# (esquisse seed)

```csharp
// VinoDellaCasa.Infrastructure / DevSeed
public static IReadOnlyList<Bottle> CreateM1Seeds(int asOfYear = 2026)
{
    // Construire les 10 Bottle ci-dessus ; après création :
    // bottle.ReadyToDrink = MaturityRules.IsReadyToDrink(
    //     bottle.Vintage, style, asOfYear);
    // Assert aligné sur le tableau §1 pour asOfYear == 2026.
}
```

---

## 4. Assertions seed (tests)

Pour `asOfYear = 2026` :

- Count == 10  
- Count(Color == Red) == 8 ; Count(Color == White) == 2  
- Count(Country == "France") == 8 ; Count(Country == "Suisse") == 2  
- ReadyToDrink true : Solitude, Fleur de Gay, Pagode, Esprit, Troplong, Clos des Abbayes, Petite Arvine (7)  
- ReadyToDrink false : Larcis, Pape Clément, Domaine de Chevalier (3)  
- PriceChf null uniquement : Esprit (#5), Clos des Abbayes (#9), Petite Arvine (#10)  
- Tous Status == InStock  
- Drink tonight top N : parmi Ready, tri Vintage ASC puis Name → plus vieux millésimes prêts en tête (ex. Fleur de Gay 2014, Troplong 2014…)

---

## 5. Hors seed M1 (volontaire)

- Rosé / Sparkling / `Color.Other` : pas de ligne seed (enums déjà couverts par le modèle).  
- Prix inventés : interdits.  
- Bouteilles hors profil goût hors les 2 blancs CH de filtre.
