# Vino della casa

Local-first **Blazor WebAssembly** wine cellar: inventory, bin locations, search/filter, and a deterministic **drink tonight** helper — runs offline in the browser via IndexedDB.

**Live demo:** https://mascot27.github.io/vino-della-casa/

## Features

- CRUD for bottles (producer, region, vintage, quantity, price, bin, status)
- Search + filters; autocomplete for varietal / region suggestions
- Deterministic maturity / “ready to drink” rules (unit-tested)
- CSV-oriented cellar workflow (export path documented in the app/docs)
- Works without an account for core cellar use (offline-first)

## Stack

| Layer | Choice |
|---|---|
| UI | .NET 8 / Blazor WASM |
| Domain | Clean-ish layering (`Domain` / `Application` / `Infrastructure` / `Web`) |
| Persistence | `ICellarStore` → IndexedDB in the browser; in-memory store for CI |
| Tests | xUnit (+ Coverlet coverage artifact on CI) |
| CI/CD | GitHub Actions on PRs; Dependabot (NuGet + Actions) |
| Hosting | GitHub Pages (static) + basic CSP |

## Run locally

```bash
dotnet test
dotnet run --project src/VinoDellaCasa.Web
```

Open the URL printed by `dotnet run`. The GitHub Pages build uses base path `/vino-della-casa/`.

## Docs

- `Docs/architecture.md` — IndexedDB vs test host
- `Docs/SPEC.md` — product & engineering notes
- `Docs/domain/` — maturity rules & sample seed notes

## Dev hygiene

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet format
```

CI uploads Coverlet cobertura as a workflow artifact. `dotnet format --verify-no-changes` runs as a soft check.
