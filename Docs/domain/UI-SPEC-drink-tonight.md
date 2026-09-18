# UI spec — À boire ce soir (Discovery)

Audience : design + impl. FR. Ranking aligné domaine (`DrinkTonight` / `MaturityRules`).

## But

Enlever le doute en ~2 secondes : proposer 3–5 bouteilles **prêtes**, en cave, avec une raison courte.

## Critères d’éligibilité

- `Status = InStock`
- `Quantity > 0`
- `ReadyToDrink = true` (recalculé via `MaturityRules` pour `asOfYear` courant si possible)

## Ordre (déjà produit)

1. Ready (déjà filtré true)
2. **Vintage ASC** (plus vieux d’abord parmi les prêts)
3. **Name** ASC

Top N = **5** (UI) ; tests peuvent utiliser 5.

## Contenu carte discovery

- Pastille couleur + badge **Prêt**
- Name · Vintage · Region
- Ligne **Pourquoi celle-ci** (template, pas de ML)

### Templates « pourquoi » (FR)

Choisir le premier qui matche :

1. Vintage ancien + prêt → `Prêt · millésime {year} (plus ancien en cave)`
2. Couleur filtrée / dominante → `Prêt · {couleur}`
3. Region présente → `Prêt · {region}`
4. Fallback → `Prêt à boire · en stock`

Pas de scores RP / pas de « coup de cœur perso ».

## Empty / loading

| État | Titre | Corps | CTA |
|---|---|---|---|
| Loading | — | Skeleton 3 cartes | — |
| Aucune prête | Rien de prêt ce soir | Les bouteilles en cave ne sont pas encore dans leur fenêtre, ou la cave est vide. | Voir la cave · Ajouter une bouteille |
| Cave vide | Cave vide | Ajoutez des bouteilles pour obtenir des suggestions. | Ajouter une bouteille |
| Filtre trop strict | Aucun résultat | Élargissez la couleur ou la région. | Réinitialiser les filtres |

## Filtres (chips, optionnel MVP+)

- Couleur : Tous / Rouge / Blanc / Rosé / Effervescent
- (Plus tard) Facile vs Occasion — seulement si un signal domaine existe (pas inventé)

## Actions

- Ouvrir détail
- **Marquer bu** depuis la carte discovery
