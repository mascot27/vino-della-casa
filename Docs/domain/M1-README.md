# Domain M1 — Index draft métier

> > Dossier : `/workspace/vino-della-casa/domain/` · 2026-09-18 (Europe/Zurich)

---

## Fichiers

| Fichier | Contenu |
|---------|---------|
| [M1-enums-draft.md](./M1-enums-draft.md) | `Color` / `BottleStatus` **déjà figés PR #1** ; focus suggestions Varietal / Region / Country |
| [M1-maturity-rules.md](./M1-maturity-rules.md) | `MaturityStyle` + fenêtres années → `ReadyToDrink` déterministe + cas xUnit |
| [M1-seed-data.md](./M1-seed-data.md) | 8 sample Bordeaux Bordeaux + 2 blancs CH ; JSON-like + asserts 2026 |
| [../taste profile (private notes, not in repo)](../taste profile (private notes, not in repo)) | Profil goût source (PDF sample Bordeaux) |
| [../SPEC.md](../SPEC.md) | Spec produit / entity Bottle |

Code de référence enums/entity : `vino-della-casa-src/src/VinoDellaCasa.Domain/` (`Color`, `BottleStatus`, `Bottle`).

---

## Décisions clés (1 écran)

1. **Enums** : ne pas renommer — `Color { Red, White, Rose, Sparkling, Other }`, `BottleStatus { InStock, Drinking, Finished }`.  
2. **Varietal / Region** : strings libres + listes de suggestions (hybrid) ; Country = noms FR.  
3. **ReadyToDrink** : `age = asOfYear − Vintage` dans `[PeakOpen, PeakClose]` par `MaturityStyle` (tables Bordeaux + CH).  
4. **Seeds** : 10 bouteilles ; prix CHF seulement si PDF sample Bordeaux ; Esprit + 2 CH → `PriceChf = null`.  
5. **Drink tonight** : filtre Ready DESC, Vintage ASC, Name — pas d’autre score M1.  
6. **MVP lean** : pas Orange, pas Classification field, pas correction millésime.

---

## Ordre d’implémentation suggéré

1. `MaturityStyle` + `MaturityRules` + tests theory (§4 maturity).  
2. Seed `CreateM1Seeds` calqué sur JSON + asserts Ready 2026.  
3. Constantes `VarietalSuggestions` / `RegionSuggestions` (UI autocomplete).  
4. Brancher recalcul Ready avant « Drink tonight ».

---

## Hors scope M1 (rappel)

OCR, sync cloud, scores RP live, champ Classification, override manuel Ready, enum Varietal fermé.
