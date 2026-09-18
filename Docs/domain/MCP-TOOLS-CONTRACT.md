# MCP `vino-della-casa` — contrat métier des tools

Serveur MCP pour agents. Labels FR. **Zéro** profil nommé / Biennois / notes perso.
Impl : alignée sur `MaturityRules`, `DrinkTonight`, `FieldSuggestions`, seeds sample.

## Principes

- Inputs JSON stricts ; outputs déterministes (même input → même output).
- `asOfYear` défaut = année civile courante (UTC+2 Europe/Zurich si le host fournit la date).
- Pas d’écriture cave cloud ; tools = **lecture / calcul** (sauf si un tool `seed_*` explicite est demandé plus tard).
- Erreurs : `{ "ok": false, "error": "code", "message": "…" }`

---

## 1. `evaluate_maturity`

**But** : calculer ReadyToDrink + style pour une bouteille.

### Input
```json
{
  "vintage": 2018,
  "color": "Red",
  "region": "Pessac-Léognan",
  "country": "France",
  "name": "Domaine de la Solitude",
  "notes": null,
  "maturityStyle": null,
  "asOfYear": 2026
}
```
- `color` : `Red|White|Rose|Sparkling|Other`
- `maturityStyle` optionnel : pin explicite (enum domaine) ; sinon heuristique `ResolveStyle`
- `vintage` nullable → ready = false, label `Sans millésime`

### Output
```json
{
  "ok": true,
  "readyToDrink": true,
  "labelFr": "Prêt",
  "maturityStyle": "PessacRougeAccessible",
  "window": { "peakOpenAge": 6, "peakCloseAge": 10 },
  "ageYears": 8,
  "whyFr": "Prêt · millésime 2018 dans la fenêtre 6–10 ans"
}
```

---

## 2. `rank_drink_tonight`

**But** : shortlist « À boire ce soir ».

### Input
```json
{
  "bottles": [ { "id": "…", "name": "…", "vintage": 2014, "color": "Red", "region": "Pomerol", "quantity": 1, "status": "InStock", "readyToDrink": null, "notes": null } ],
  "topN": 5,
  "asOfYear": 2026,
  "recalculateReady": true,
  "colorFilter": null
}
```
- `status` : `InStock|Drinking|Finished`
- Si `recalculateReady` : appeler logique maturité avant filtre
- Éligible : InStock, qty>0, ready=true (+ colorFilter si set)

### Output
```json
{
  "ok": true,
  "items": [
    {
      "id": "…",
      "name": "…",
      "vintage": 2014,
      "region": "Pomerol",
      "color": "Red",
      "colorLabelFr": "Rouge",
      "readyLabelFr": "Prêt",
      "whyFr": "Prêt · millésime 2014 (plus ancien en cave)",
      "rank": 1
    }
  ],
  "emptyReasonFr": null
}
```
- Tri : vintage ASC, name ASC
- Si vide : `items=[]`, `emptyReasonFr` parmi les copy UI-SPEC-drink-tonight

---

## 3. `list_seed_bottles`

**But** : sample data anonyme (démo / tests agents).

### Input
```json
{ "asOfYear": 2026, "includeReadyOnly": false }
```

### Output
```json
{
  "ok": true,
  "count": 10,
  "readyCount": 7,
  "bottles": [ /* Bottle sample fields + readyToDrink + colorLabelFr */ ]
}
```
- Sources : `M1SeedData` (noms de vins publics OK ; pas de « profil utilisateur »)

---

## 4. `suggest_varietal_region`

**But** : autocomplete suggestions (strings libres).

### Input
```json
{
  "field": "varietal",
  "query": "neb",
  "limit": 8
}
```
- `field` : `varietal|region|country`
- `query` : substring case-insensitive ; vide = top liste

### Output
```json
{
  "ok": true,
  "field": "varietal",
  "suggestions": ["Nebbiolo", "…"]
}
```
- Listes : Bordeaux + CH + Italie (Sangiovese, Nebbiolo, …) + Rhône + Rosé courants — **génériques**, pas un profil perso

---

## 5. `get_color_labels` (petit, utile UI/agents)

### Input
`{}`

### Output
```json
{
  "ok": true,
  "colors": [
    { "value": "Red", "labelFr": "Rouge", "hex": "#6B1E2A" },
    { "value": "White", "labelFr": "Blanc", "hex": "#C9A227" },
    { "value": "Rose", "labelFr": "Rosé", "hex": "#E8A0A8" },
    { "value": "Sparkling", "labelFr": "Effervescent", "hex": "#E8D5A3" },
    { "value": "Other", "labelFr": "Autre", "hex": "#6B635C" }
  ],
  "maturityBadges": [
    { "key": "ready", "labelFr": "Prêt", "hex": "#2F6B4F" },
    { "key": "wait", "labelFr": "À attendre", "hex": "#8A6A2F" }
  ]
}
```

---

## Critères DoD (pour implementation / project tracking)

1. Chaque tool ci-dessus exposé + schema JSON input
2. Parité tests avec xUnit domaine (surtout maturity windows + 7 ready / 3 young sur seeds 2026)
3. Zéro string meta (agents, profil nommé, Biennois)
4. README MCP : comment brancher le serveur (stdio/SSE selon choix repo)
5. Hors scope v1 : OAuth Millésima, écriture IndexedDB distante, achats

## Hors scope explicite

- Scraping compte marchand
- Recommandations prix inventées
- Enum Varietal fermé (reste suggestions)
