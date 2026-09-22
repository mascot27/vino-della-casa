# Accords mets ↔ bouteille — règles métier

Produit-only. Moteur déterministe, offline. **Pas** de prose critique ni de blog.

Public lite et privé partagent le même moteur (`PairingEngine`) ; le privé peut enrichir densités taste / scores sans changer l’API.

## Entrées

Depuis une fiche catalogue / profil goût :

| Champ | Usage |
|-------|--------|
| `color` | Red / White / Rose / Sparkling |
| `taste.body` | light / medium / full |
| `taste.tannin` | soft / medium / firm (rouges) |
| `taste.acidity` | low / medium / high |
| `taste.oak` | none / subtle / marked |
| `taste.pairingHints` | hints libres (boost si overlap) |
| `region` / `styleTags` | affinage (ex. Chablis → iodé) |
| `readyToDrink` | filtre soft (préférer prêts) |

Quand le seed n’a pas de taste structuré (démo Bordeaux), le profil est **inféré** depuis couleur × région × style (heuristique stable, testée).

## Sortie

Liste de **tags mets** (labels FR UI) + score 0–100 + raison courte (1 phrase template) :

```json
{ "dish": "agneau rôti", "score": 86, "why": "Structure tannique + corps plein" }
```

## Tables de base (couleur)

### Rouge

| Profil | Accords prioritaires |
|--------|----------------------|
| light + soft tannin | volaille rôtie, charcuterie, champignons, pizza |
| medium | agneau, bœuf grillé, pâtes viande, comté |
| full + firm | bœuf mijoté, gibier, côte de bœuf, fromages affinés |
| marked oak | bœuf grillé, magret, chocolat noir (dessert rare) |

### Blanc

| Profil | Accords prioritaires |
|--------|----------------------|
| high acid + light (Chablis-like) | huîtres, fruits de mer, chèvre frais |
| medium acid + subtle oak | volaille crème, veau, Comté jeune |
| full + marked oak (Meursault-like) | homard, ris de veau, poularde |
| aromatic / off-dry tags | cuisine asiatique douce, fromage bleu doux |

### Rosé

apéritif, grillades légères, salades, cuisine méditerranéenne, pizza

### Effervescent

apéritif, fruits de mer, sushis, fritures légères, desserts fruités (pas trop sucrés)

## Affinages région (bonus +5 à +15)

| Signal région / tags | Boost dishes |
|----------------------|--------------|
| Chablis, iodé, flint | huîtres, fruits de mer |
| Beaujolais, gamay, easy | charcuterie, volaille |
| Saint-Émilion / Pomerol / merlot-soft | agneau, canard |
| Médoc / cabernet / firm | agneau, bœuf grillé |
| Sauternes / liquoreux | foie gras, bleu |
| Rhône sud / grenache | grillades, ratatouille |
| Provence rosé | salades, grillades |

## Algorithme (v1)

1. Base score depuis table **couleur × body/tannin/acidity** (et oak).
2. +bonus région / `styleTags` ; +bonus si `pairingHints` overlap (case-insensitive, synonyme EN↔FR).
3. −pénalité soft si millésime trop jeune (rouge de garde non prêt).
4. Top N (défaut 5) dishes **ou** top N bottles for a given dish (symétrique).

Deux modes UI :

- **Bouteille → mets** — fiche catalogue détail
- **Plat → bouteilles** — page Accords (catalogue seed)

## Hors scope v1

- Nutrition / allergies / recettes complètes
- Scraping blogs accords
- LLM obligatoire (templates FR fixes)

## DoD métier

- [x] Tables ci-dessus en code (`PairingEngine`) testables
- [x] Labels FR stables (`UiLabels`)
- [x] 10 fixtures (ex. Solitude rouge → grillades/agneau ; Chablis → huîtres)
- [x] Même API domaine pour public (+ privé ultérieur)
