# UI spec — Tokens & labels FR

Produit-only. Aligné cartes + discovery.

## Labels FR (courts)

| Concept | Label |
|---|---|
| Red | Rouge |
| White | Blanc |
| Rose | Rosé |
| Sparkling | Effervescent |
| Other | Autre |
| Ready | Prêt |
| Not ready | À attendre |
| No vintage | Sans millésime |
| InStock | En cave |
| Drinking | En cours |
| Finished | Fini |
| Drink tonight | À boire ce soir |
| Cellar | Cave |
| Bin | Casier |
| Quantity | Quantité |
| Varietal | Cépage |
| Region | Région |
| Mark drunk | Marquer bu |
| Add bottle | Ajouter une bouteille |
| Filter | Filtrer |
| Pairings | Accords |

## Tokens couleur app

| Token | Hex | Usage |
|---|---|---|
| `--bg` | `#F7F3EE` | Fond |
| `--surface` | `#FFFFFF` | Cartes |
| `--text` | `#1F1A17` | Texte principal |
| `--text-muted` | `#6B635C` | Secondaire |
| `--accent-ready` | `#2F6B4F` | Badge Prêt |
| `--accent-wait` | `#8A6A2F` | Badge À attendre |
| `--color-red` | `#6B1E2A` | Pastille rouge |
| `--color-white` | `#C9A227` | Pastille blanc |
| `--color-rose` | `#E8A0A8` | Pastille rosé |
| `--color-sparkling` | `#E8D5A3` | Pastille effervescent |

## Motion

- Hover lift ~2–4px, shadow soft
- `prefers-reduced-motion: reduce` → aucune transform/anim d’entrée

## Contraintes repo

- Base path `/vino-della-casa/`
- CSP soft inchangée
- Zéro meta perso / agents / profil nommé dans copy UI
