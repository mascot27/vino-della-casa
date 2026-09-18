# UI spec — Cartes bouteille (produit)

Audience : design + impl. FR. Aucune donnée perso / profil nommé.

## But

Remplacer le look « table admin » par des cartes lisibles en 1 coup d’œil : **quoi**, **quand** (maturité), **où** (région / casier).

## Champs visibles (priorité)

| Priorité | Champ | Affichage |
|---|---|---|
| 1 | Couleur | Pastille (Rouge / Blanc / Rosé / Effervescent) |
| 1 | ReadyToDrink | Badge **Prêt** ou **À attendre** |
| 1 | Name | Titre principal |
| 1 | Vintage | Sous le nom, gros (ex. `2018`) ; optionnel `· 8 ans` si asOfYear connu |
| 2 | Region | Ligne secondaire (appellation / région) |
| 3 | Producer | Si place ; sinon détail |
| 3 | Bin | Chip casier (ex. `A1`) |
| 4 | Quantity | Chip `×2` |
| 4 | Varietal | Détail / reverse carte, pas sur la face MVP |
| 5 | PriceChf | Secondaire uniquement |

Ne pas afficher sur la carte MVP : scores critiques, `MaturityStyle=` technique, notes brutes.

## Hiérarchie typographique

1. Pastille couleur · badge maturité (scan rapide, au-dessus du titre)
2. **Name** (semibold)
3. **Vintage** (+ âge optionnel)
4. **Region** (muted)
5. Meta chips : casier · qty · prix (secondaire)

## Pastille couleur

| Color enum | Label FR | Hex pastille | Texte sur pastille |
|---|---|---|---|
| Red | Rouge | `#6B1E2A` | `#FFFFFF` |
| White | Blanc | `#C9A227` | `#1A1208` |
| Rose | Rosé | `#E8A0A8` | `#3B1518` |
| Sparkling | Effervescent | `#E8D5A3` | `#2A2110` |
| Other | Autre | `#6B635C` | `#FFFFFF` |

## Badge maturité

| État | Label | Hex | Texte |
|---|---|---|---|
| ReadyToDrink = true | Prêt | `#2F6B4F` | `#FFFFFF` |
| ReadyToDrink = false + Vintage set | À attendre | `#8A6A2F` | `#FFFFFF` |
| Vintage null | Sans millésime | `#6B635C` | `#FFFFFF` |

Ne jamais réutiliser la teinte « Rouge » pour « Prêt ».

## Interaction

- Hover : lift + shadow soft (`prefers-reduced-motion: reduce` → pas d’anim)
- Entrée liste : fade/slide court, désactivable reduced-motion
- Tap carte → détail / édition
- Action rapide optionnelle : **Marquer bu** (qty−1)

## Accessibilité

- Pastille + texte label (pas couleur seule)
- Contraste AA sur pastilles / badges (paires ci-dessus)
- Focus ring visible (token focus)
