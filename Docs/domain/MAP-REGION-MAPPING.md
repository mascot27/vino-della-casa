# Mapping appellation / région → carte France (filtre Cave)

Produit-only. Match case-insensitive sur `Bottle.Region` (+ Country pour CH).

## Régions carte (hotspots)

| Map region id | Label FR UI | Match si Region contient (exemples) |
|---|---|---|
| `bordeaux` | Bordeaux | Pessac, Léognan, Graves, Pomerol, Saint-Émilion, Saint-Emilion, Médoc, Medoc, Haut-Médoc, Pauillac, Margaux, Saint-Julien, Saint-Estèphe, Saint-Estephe, Sauternes, Barsac, Fronsac, Castillon, Lalande, Bourg, Blaye, Entre-deux-Mers, Entre-Deux-Mers, Cadillac, Loupiac, Cérons, Bordeaux |
| `bourgogne` | Bourgogne | Bourgogne, Burgundy, Côte de Nuits, Cote de Nuits, Côte de Beaune, Cote de Beaune, Chablis, Beaujolais, Mâcon, Macon, Côte Chalonnaise |
| `champagne` | Champagne | Champagne, Reims, Épernay, Epernay |
| `loire` | Loire | Loire, Sancerre, Pouilly, Vouvray, Chinon, Bourgueil, Muscadet, Anjou, Saumur, Touraine |
| `alsace` | Alsace | Alsace, Riesling, Gewurz (si Region=Alsace…), Colmar |
| `rhone` | Rhône | Rhône, Rhone, Côte-Rôtie, Cote-Rotie, Hermitage, Crozes, Châteauneuf, Chateauneuf, Gigondas, Condrieu, Cornas, Vacqueyras, Tavel |
| `provence` | Provence | Provence, Bandol, Côtes de Provence, Coteaux d’Aix, Palette, Cassis |
| `sud-ouest` | Sud-Ouest | Madiran, Cahors, Jurançon, Jurancon, Bergerac, Gaillac, Fronton, Marcillac, Irouléguy, Irouleguy, Buzet |
| `suisse` | Suisse | Valais, Vaud, Lavaux, Genève, Geneve, Tessin, Ticino, Neuchâtel, Neuchatel, Suisse, Switzerland *(aussi si Country = Suisse)* |

## Règles

1. Premier match gagne (ordre du tableau).
2. Si `Country` ∈ {Suisse, Switzerland, Schweiz} → `suisse` même si Region vague.
3. Compteur région = bouteilles `InStock` avec qty>0 matchant la région.
4. Clic « Tous » = clear filter.
5. Hors carte (Italie, Espagne…) : pas de hotspot v1 — restent visibles uniquement en « Tous » ou via search #46.

## Seed demo attendu

- Bordeaux : majorité `demo-sample-50.json`
- À ajouter si manquant : Petite Arvine (Valais → suisse), Minuty (Provence), 1 Champagne/Prosecco (Champagne ou hors carte)

