# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- Catalogue fiche hero: photo-only (removed bottle silhouette overlay that read as a color bar) (#61).

### Added

- Accords mets ↔ bouteille (#73): `PairingEngine` (couleur × body/tannin/acidity + région + hints), docs `PAIRING-RULES.md`, suggestions sur fiche catalogue + page `/accords` (plat → bouteilles), 10 fixtures xUnit.

- Catalogue détail assemblage: chips `%` via `blendsByVintage` (prefer fiche millésime); fallback plain Blend list when absent (#68).

- Catalogue Bourgogne (+100 anonymous fiches) wired into `CatalogSeedData` (~153 total); null `imageKey` → local `img/catalog/estate-*.jpg`; map filter hotspot Bourgogne (#63).

- Catalogue fiche détail (`/catalogue/{id}`): full letter page with assemblage, classement, fenêtre, accords, scores/prix optionnels, notes cave locales; list card click → detail (#60).
- Catalogue-first Cave (#55): `CatalogEntry` + `CatalogSeedData` (53 anonymous fiches), `CatalogFiche` letter layout, local `wwwroot/img/catalog/` heroes + ATTRIBUTION, « J’en ai en cave » qty → IndexedDB via `CellarService.SetCatalogQuantityAsync`.

### Changed

- Bottle cards v2: local SVG silhouettes by color, hover lift/shadow/silhouette scale, discreet secondary actions, coherent empty/skeleton states.
- Bottle cards (cellar + drink-tonight): color pill + maturity badge above the title for faster visual scan.

### Added

- MCP stdio server v1 (`src/VinoDellaCasa.Mcp`): `evaluate_maturity`, `rank_drink_tonight`, `list_seed_bottles`, `suggest_varietal_region`, `get_color_labels` — read-only Domain tools; contract in `Docs/domain/MCP-TOOLS-CONTRACT.md`.
- M1.5: GitHub Pages deploy workflow (`deploy-pages.yml`), project `<base href="/vino-della-casa/" />`, basic CSP in `index.html`, `.nojekyll`.
- M1: `MaturityStyle` / `MaturityRules` (deterministic ReadyToDrink) + xUnit coverage.
- M1: SampleBordeaux Bordeaux + CH white seeds (`M1SeedData`), 7 ready / 3 too young as of 2026.
- M1: `CellarService` CRUD façade, Drink tonight shortlist, Varietal/Region/Country suggestions.
- M1: Blazor cellar CRUD + Drink tonight UI; IndexedDB `ICellarStore` path for WASM; in-memory for CI.
- M1: Domain draft docs under `Docs/domain/`.
- M0 scaffold: solution layout, Domain (`Bottle`, enums), `ICellarStore`, empty Blazor WASM shell, CI workflow, docs.
