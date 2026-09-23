# Comparateur « même esprit » — règles métier

Produit-only. Moteur déterministe, offline. **Pas** de ML ni de LLM.

Réutilise `WinePairingProfile` / taste (couleur, body, tannin, acidity, oak, région, styleTags) via `PairingProfileBuilder`. Public lite et privé partagent le même moteur (`SimilarityEngine`).

## Entrée / sortie

- **Entrée** : une bouteille (catalogue ou cave) + pool de candidats
- **Sortie** : top N (défaut 5, UI 3–5) `{ id, score 0–100, why }` — pourquoi court FR template

## Score (somme bornée 100)

| Signal | Poids max | Règle |
|--------|-----------|--------|
| Couleur | 30 | même couleur = 30 ; sinon 0 (**exclure** autre couleur sauf Sparkling↔White soft 10) |
| Body | 20 | exact 20 ; adjacent ±1 = 10 |
| Tannin (rouges) | 15 | exact 15 ; adjacent 8 ; blancs/rosés/effervescents = 0 (N/A) |
| Acidity | 15 | exact 15 ; adjacent 8 |
| Oak | 10 | exact 10 ; adjacent 5 |
| Région / famille | 15 | même appellation 15 ; même hotspot carte (bordeaux / bourgogne / …) 10 ; sinon 0 |
| styleTags overlap | 10 | +5 par tag commun (cap 10) |

Pénalités soft :

- millésime très éloigné si les deux ont un vintage (−5 si \|Δ\| > 8)
- candidate non prête alors que la source est prête (−5)

## Why templates (ex.)

- « Même couleur et structure (corps plein, tanins fermes) »
- « Même famille Bordeaux, profil proche »
- « Style Chablis / minéral voisin »
- « Même couleur, structure proche, Bourgogne »

## Hors scope v1

- Prix comme critère de similarité (filtre UI optionnel à part)
- Scores critiques comme similarité (affichage seulement)
- LLM / embeddings

## DoD métier

- [x] Engine testable + fixtures (Solitude → autre Pessac ; Chablis → autre Chablis / blanc iodé ; Pomerol → rive droite ; Sauternes ↛ Médoc sec)
- [x] UI fiche : bloc « Dans le même esprit » → liens fiches
- [x] Même API domaine pour public (+ privé ultérieur)
