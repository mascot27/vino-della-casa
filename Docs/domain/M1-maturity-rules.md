# M1 — Règles de maturité / ReadyToDrink (draft métier)

> Déterministe, testable en xUnit · Pas de ML · Réf. millésime = année de récolte  
> Année d’évaluation `asOfYear` (tests : injecter ; prod : `DateTimeOffset.UtcNow.Year` ou horloge locale)  
> Date draft : 2026-09-18 (Europe/Zurich)

---

## 1. Contrat

```text
ReadyToDrink = true  ⇔  PeakOpenAge ≤ (asOfYear − Vintage) ≤ PeakCloseAge
```

- Si `Vintage` est null → `ReadyToDrink = false` (MVP : pas de fenêtre sans millésime).
- Âges en **années civiles complètes** depuis le millésime (ex. millésime 2018, asOf 2026 → âge 8).
- Les fenêtres sont des **ordres de grandeur de style**, pas des scores RP individuels. Les notes producteur/RP des sample Bordeaux servent à **calibrer** les tables, pas à hardcoder chaque château.
- `ReadyToDrink` sur `Bottle` peut être **recalculé** à l’écriture / au seed / avant « Drink tonight » (pure function domaine).

### API domaine proposée

```csharp
public enum MaturityStyle
{
    PessacRougeAccessible,   // ex. Dom. de la Solitude
    PessacRougeSecondVin,    // ex. Esprit de Chevalier
    PessacRougeClasse,       // Cru classé Graves / Pessac (Pape Clément, Dom. de Chevalier)
    PomerolMerlot,           // Pomerol Merlot-dominant
    StEmilionPremierGcc,     // 1er GCC (Larcis, Troplong…)
    StEmilionGcc,            // GCC / grand cru générique
    MedocStEstephe,          // Saint-Estèphe / Médoc CS-dominant (1er vin)
    MedocSecondVin,          // 2e vin Médoc / St-Estèphe (Pagode de Cos)
    BlancSecBordeaux,        // Pessac / Graves blanc
    BlancSecSuisse,          // Chasselas, Petite Arvine…
    RougeSuisse,             // Pinot, Merlot Tessin, Cornalin…
    RougeGenerique,          // fallback rouge structuré
    BlancGenerique,          // fallback blanc sec
    RoseOuEffervescent       // boire jeune
}

public readonly record struct MaturityWindow(int PeakOpenAge, int PeakCloseAge);

public static class MaturityRules
{
    public static MaturityWindow WindowFor(MaturityStyle style) { /* table */ }

    public static bool IsReadyToDrink(int? vintage, MaturityStyle style, int asOfYear)
    {
        if (vintage is null) return false;
        var age = asOfYear - vintage.Value;
        var w = WindowFor(style);
        return age >= w.PeakOpenAge && age <= w.PeakCloseAge;
    }
}
```

Résolution `MaturityStyle` depuis une bouteille : heuristique simple (Region + Color + Notes/Name keywords) — table §3 — ou override manuel au seed.

---

## 2. Tables de fenêtres (années depuis vintage)

Bornes **inclusives**. Calibrées sur profil Bordeaux structuré + repères `styles-et-service.md`.

| Style | PeakOpen | PeakClose | Calibrage sample Bordeaux / notes |
|-------|----------|-----------|----------------------------|
| `PessacRougeAccessible` | 6 | 10 | Solitude 2018 → 2024–2028 |
| `PessacRougeSecondVin` | 6 | 12 | Esprit 2019 → dès ~2025 |
| `PessacRougeClasse` | 8 | 16 | Pape Clément 2019 → 2027–2034 ; Chevalier 2020 garde |
| `PomerolMerlot` | 8 | 18 | La Fleur de Gay 2014 musculaire / garde |
| `StEmilionPremierGcc` | 7 | 20 | Larcis 2020 → 2027–2040 ; Troplong 2014 → 2022–2030 |
| `StEmilionGcc` | 6 | 14 | GCC / grand cru hors 1er |
| `MedocStEstephe` | 8 | 18 | 1er vin St-Estèphe / Médoc CS |
| `MedocSecondVin` | 5 | 12 | Pagode de Cos 2019 (2e vin) |
| `BlancSecBordeaux` | 3 | 10 | Pessac blanc sec |
| `BlancSecSuisse` | 1 | 5 | Chasselas / Arvine de soif |
| `RougeSuisse` | 2 | 8 | Pinot / Merlot Tessin / Cornalin |
| `RougeGenerique` | 5 | 12 | fallback rouge |
| `BlancGenerique` | 1 | 6 | fallback blanc |
| `RoseOuEffervescent` | 0 | 3 | boire jeune |

### Snapshot « asOfYear = 2026 » (seeds sample Bordeaux)

| Vin | Vintage | Style | Âge | Ready? |
|-----|---------|-------|-----|--------|
| Domaine de la Solitude | 2018 | PessacRougeAccessible | 8 | **true** |
| Château La Fleur de Gay | 2014 | PomerolMerlot | 12 | **true** |
| Château Larcis Ducasse | 2020 | StEmilionPremierGcc | 6 | **false** (ouvre 7 → 2027) |
| Pagode de Cos | 2019 | MedocSecondVin | 7 | **true** |
| L’Esprit de Chevalier | 2019 | PessacRougeSecondVin | 7 | **true** |
| Château Pape Clément | 2019 | PessacRougeClasse | 7 | **false** (ouvre 8 → 2027) |
| Château Troplong Mondot | 2014 | StEmilionPremierGcc | 12 | **true** |
| Domaine de Chevalier | 2020 | PessacRougeClasse | 6 | **false** |

---

## 3. Résolution de style (heuristique MVP)

Ordre d’évaluation (premier match gagne) :

1. `Color` ∈ { Rose, Sparkling } → `RoseOuEffervescent`
2. `Color == White` + Country/Region Suisse (Valais, Vaud, Lavaux, Genève, Tessin…) → `BlancSecSuisse`
3. `Color == White` + Region Pessac/Graves/Bordeaux → `BlancSecBordeaux`
4. `Color == White` → `BlancGenerique`
5. Region contient `Pomerol` → `PomerolMerlot`
6. Region `Saint-Émilion` + Notes/Name contient `1er` / `Premier Grand Cru` → `StEmilionPremierGcc`
7. Region `Saint-Émilion` → `StEmilionGcc`
8. Region `Saint-Estèphe`|`Pauillac`|`Médoc`|`Haut-Médoc`|`Margaux`|`Saint-Julien` + Name/Notes contient `2e vin` / `Pagode` / `Esprit` style second → `MedocSecondVin`  
   (sinon si St-Estèphe/Médoc…) → `MedocStEstephe`
9. Region `Pessac-Léognan`|`Graves` + Notes contient `Cru classé` / Name cru classé connu → `PessacRougeClasse`  
   + Name/Notes `2e` / `Esprit` → `PessacRougeSecondVin`  
   sinon → `PessacRougeAccessible`
10. Country/Region Suisse + Red → `RougeSuisse`
11. Red → `RougeGenerique`

Seeds M1 **fixent le style explicitement** (pas d’ambiguïté pour les tests).

---

## 4. Cas de tests unitaires (obligatoires)

```text
Theory: IsReadyToDrink
  (2018, PessacRougeAccessible, 2026) → true   // âge 8 ∈ [6,10]
  (2018, PessacRougeAccessible, 2023) → false  // âge 5 < 6
  (2018, PessacRougeAccessible, 2029) → false  // âge 11 > 10
  (2020, StEmilionPremierGcc, 2026) → false    // âge 6 < 7
  (2020, StEmilionPremierGcc, 2027) → true
  (2014, PomerolMerlot, 2026) → true           // âge 12 ∈ [8,18]
  (2019, PessacRougeClasse, 2026) → false      // âge 7 < 8
  (2019, MedocSecondVin, 2026) → true          // âge 7 ∈ [5,12]
  (null, RougeGenerique, 2026) → false
  (2024, RoseOuEffervescent, 2026) → true      // âge 2 ∈ [0,3]
  (2020, BlancSecSuisse, 2026) → false         // âge 6 > 5
  (2023, BlancSecSuisse, 2026) → true          // âge 3 ∈ [1,5]
```

WindowFor : assert PeakOpen/PeakClose pour chaque `MaturityStyle` (table §2).

---

## 5. Lien « Drink tonight »

SPEC : sort `(ReadyNow DESC, Vintage ASC, Name)` → top N.

- `ReadyNow` ≡ `Bottle.ReadyToDrink` (bool déjà sur l’entité).
- Recalculer avant la query si l’horloge a changé d’année (ou au load store).
- Pas d’autre score magique en M1.

---

## 6. Limites assumées (MVP lean)

- Pas de correction millésime (2015 vs 2018 Bordeaux).  
- Pas de stockage condition (cave idéale vs placard).  
- Override manuel : l’utilisateur peut cocher/décocher `ReadyToDrink` ; un flag futur `ReadyToDrinkManual` est **hors M1** — pour l’instant le seed / service peut écraser au recalcul (documenter dans README app).
