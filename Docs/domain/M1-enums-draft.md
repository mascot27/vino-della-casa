# M1 — Suggestions champs libres (draft métier)

> Projet **Vino della casa** · Milestone M1 · FR  
> Aligné sur entity `Bottle` PR #1 + `SPEC.md` · Profil goût (`taste profile (private notes, not in repo)`)  
> Date draft : 2026-09-18 (Europe/Zurich)

---

## 0. Déjà figé en code (ne pas réinventer)

Source locale : `vino-della-casa-src/.../Enums/` + `Entities/Bottle.cs`.

### `Color` (enum)

```csharp
public enum Color { Red, White, Rose, Sparkling, Other }
```

| Valeur | Usage MVP |
|--------|-----------|
| Red | home cellar (Bordeaux) |
| White | Seeds / filtres CH + blancs |
| Rose | Filtre UI ; 0 seed obligatoire |
| Sparkling | Champagne / Crémant |
| Other | Filet de sécurité CSV / cas rares |

**Pas Orange en MVP** — `Other` couvre les outliers. Affichage UI : Rose → « Rosé », Sparkling → « Effervescent ».

### `BottleStatus` (enum)

```csharp
public enum BottleStatus { InStock, Drinking, Finished }
```

Inchangé. Transitions via use cases (`Drink`, `Finish`, …).

### Champs `Bottle` déjà typés

| Champ | Type code | Note draft |
|-------|-----------|------------|
| Color | `Color` | figé |
| Status | `BottleStatus` | figé |
| Varietal | `string?` | **suggestions ci-dessous** |
| Region | `string?` | **suggestions ci-dessous** |
| Country | `string?` | noms FR canoniques |
| ReadyToDrink | `bool` | calcul / règles → `M1-maturity-rules.md` |

---

## 1. `Varietal` — hybrid (string libre + suggestions)

### Décision

- **Pas d’enum** `Varietal` en M1.
- Stockage libre (`string?`) pour assemblages Bordeaux et orthographes.
- UI / Application : constante `VarietalSuggestions` (autocomplete, pas contrainte DB).

### Suggestions MVP (priorité taste profile + CH)

**Rouges prioritaires**  
`Cabernet Sauvignon`, `Merlot`, `Cabernet Franc`, `Petit Verdot`, `Pinot Noir`, `Gamay`, `Syrah`, `Cornalin`, `Humagne Rouge`

**Rouges secondaires**  
`Nebbiolo`, `Sangiovese`, `Tempranillo`, `Malbec`, `Grenache`

**Blancs**  
`Chardonnay`, `Sauvignon Blanc`, `Sémillon`, `Riesling`, `Chasselas`, `Petite Arvine`, `Chenin Blanc`, `Pinot Gris`, `Gewurztraminer`, `Viognier`, `Grüner Veltliner`

**Assemblages (exemples libres, pas dans la liste fermée)**  
`"CS 65 / Merlot 27 / PV 5 / CF 3"`, `"Merlot, Cabernet Franc"`, `"CS, Merlot, Petit Verdot"`

---

## 2. `Region` — string libre + suggestions

Champ nullable en code ; seeds / UI le renseignent toujours quand connu.

### Bordeaux (priorité goût — ordre UX)

1. Pessac-Léognan  
2. Saint-Émilion  
3. Pomerol  
4. Saint-Estèphe  
5. Pauillac  
6. Margaux  
7. Saint-Julien  
8. Médoc / Haut-Médoc  
9. Graves  
10. Sauternes  
11. Bordeaux (générique)

### Suisse

1. Valais  
2. Vaud  
3. Lavaux  
4. Tessin  
5. Genève  
6. Neuchâtel  
7. Trois Lacs  
8. Grisons  

### Secondaire

Bourgogne, Champagne, Loire, Rhône, Alsace, Beaujolais, Toscane, Piémont, Rioja

**Classement** (1er GCC, Cru classé Graves…) → `Notes` ou `Name`, **pas** de champ Classification M1.  
Exemple : `Region = "Saint-Émilion"`, `Notes = "1er Grand Cru Classé 2022 · RP 96"`.

---

## 3. `Country` — noms FR

- Stockage : nom FR (`France`, `Suisse`, …) — pas de code ISO en M1.
- Seeds sample Bordeaux → `France` ; compléments CH → `Suisse`.
- Suggestions : France, Suisse, Italie, Espagne, Allemagne, Autriche, Portugal, États-Unis, Chili, Argentine, Afrique du Sud, Australie, Nouvelle-Zélande.

---

## 4. À calquer pour implementation

1. **Ne pas** renommer `Color` / `BottleStatus` ni retirer `Other`.  
2. Varietal = string libre + `VarietalSuggestions`.  
3. Region / Country = strings FR + listes suggestions.  
4. ReadyToDrink = bool (règles déterministes → `M1-maturity-rules.md`).  
5. Seeds → `M1-seed-data.md`.
