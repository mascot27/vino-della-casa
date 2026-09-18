# Architecture — Vino della casa

## Layers

| Project | Role |
|---|---|
| `VinoDellaCasa.Domain` | Entities (`Bottle`), enums (`Color`, `BottleStatus`), pure maturity (`MaturityRules`), seeds, suggestions — **no** Blazor / EF references |
| `VinoDellaCasa.Application` | Abstractions (`ICellarStore`), façade (`CellarService`) |
| `VinoDellaCasa.Infrastructure` | Persistence adapters |
| `VinoDellaCasa.Web` | Blazor WebAssembly UI |

## Persistence (offline-first)

**Primary (browser / PWA):** `ICellarStore` → **`IndexedDbCellarStore`** → IndexedDB.

| Piece | Location |
|---|---|
| Contract | `Application/Abstractions/ICellarStore.cs` |
| WASM adapter | `Infrastructure/Persistence/IndexedDbCellarStore.cs` |
| JS API | `Web/wwwroot/js/cellarStore.js` (`vinoCellarStore.*`) |
| DI | `Web/Program.cs` registers `IndexedDbCellarStore` |

- Not `localStorage` (quota limits and fragile JSON blobs).
- DB name `VinoDellaCasa`, object store `bottles`, keyPath `id`.
- No OAuth / no cloud account — data stays in the browser profile.

**Secondary (tests / CI):** `InMemoryCellarStore` (test double).

- Keeps `dotnet test` and CI green without a browser.
- Optional later: EF Core + SQLite host path for reviewers.

```
Browser (Blazor WASM) ──► ICellarStore ──► IndexedDbCellarStore ──► IndexedDB
Tests / CI             ──► ICellarStore ──► InMemoryCellarStore
```

### ReadyToDrink recalculation

`CellarService` / `DrinkTonight` call pure `MaturityRules.ApplyReadyToDrink` (asOfYear) before shortlist and on write. Seeds pin `MaturityStyle=…` in `Notes`; otherwise a Region/Color heuristic applies. Manual override flag is **out of scope for M1** — recalculation may overwrite a UI checkbox.

## Dependency rule

Domain ← Application ← Infrastructure / Web. Domain never references outer layers.

## M1 status

- MaturityRules + xUnit (seed Ready cases)
- SampleBordeaux + CH seeds
- CRUD UI + Drink tonight shortlist
- Autocomplete Varietal / Region / Country (string suggestions)
- IndexedDB path wired for WASM; in-memory for CI
- No OAuth/auth


## Hosting (M1.5)

- **GitHub Pages** project site: `https://mascot27.github.io/vino-della-casa/`
- `wwwroot/index.html` uses `<base href="/vino-della-casa/" />` so Blazor asset URLs resolve under that path.
- Basic CSP meta tag (soft): `default-src 'self'` with `script-src` including `'wasm-unsafe-eval'` for Blazor WASM, and `style-src 'self' 'unsafe-inline'`.
- Workflow: `.github/workflows/deploy-pages.yml` (`dotnet publish` Release → `upload-pages-artifact` → `deploy-pages`). Existing `.github/workflows/ci.yml` is unchanged.
- `.nojekyll` in `wwwroot` so Pages serves `_framework/` (underscore paths).
