# Catalogue Bourgogne 100 (seed anonyme)

Issue: [vino-della-casa#63](https://github.com/mascot27/vino-della-casa/issues/63)  
`asOfYear=2026` · ready **60** / wait **40**

## Chemins

| Fichier | Rôle |
|---|---|
| `domain/catalog-bourgogne-100.json` (copie locale) / `Docs/domain/catalog-bourgogne-100.json` (repo) | Seed catalogue app |
| `wine-knowledge/bourgogne-db/demo/catalog-bourgogne-100.json` | Miroir knowledge |

## Source

- `wine-knowledge/bourgogne-db/profiles/estates.jsonl` (~1300 profils goût)
- `pricing.amount` (EUR) → `priceEur` + `priceChf` (approx. 1:1) si présent
- Scores numériques compactés uniquement (`rp` / `js` / `ws` / `hachetteStars`) — **aucune** prose critique

## Sélection (diversité)

| Sous-région | n |
|---|---|
| Chablis | 15 |
| Côte de Nuits | 25 |
| Côte de Beaune | 30 |
| Côte Chalonnaise | 12 |
| Mâconnais | 12 |
| Hautes-Côtes | 6 |

- Préférence: taste complet → pricing → scores
- Max ~2 cuvées par producteur
- Mix rouge (Pinot noir) / blanc (Chardonnay)
- `quantity=0`, `status=InStock` (catalogue-first, pas de stock cave)

## Scrub (anonymat)

1. Retirer tags perso / événement privé / id commande / email dans `styleTags`
2. `pourquoi` et `styleLine` : phrases métier FR génériques uniquement
3. Grep final JSON : zéro motif perso du brief #63

Garder uniquement tags de style (`mineral`, `premier-cru`, `chablis-style`, etc.).

## Carte (hotspot)

- Top-level `mapRegionHint`: `"bourgogne"`
- Chaque bouteille: `mapRegion`: `"bourgogne"`
- `region`: appellation, ou `village, subRegion` pour matcher `MAP-REGION-MAPPING.md`

## Images

- `imageKey`: **null** (labels restent sous `bourgogne-db/media/labels/`)
- VinoDev peut mapper un sous-ensemble vers `wwwroot/img/catalog/` + ATTRIBUTION plus tard

## ReadyToDrink

1. Si `taste.drinkWindowYears` → `open ≤ (2026 − vintage) ≤ close`
2. Sinon: blanc ≈ 2–10 ans ; rouge village / 1er / grand cru fenêtres plus longues
