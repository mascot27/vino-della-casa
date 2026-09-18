# Vino della casa

Local-first Blazor WebAssembly wine cellar: stock, bin locations, search, and a deterministic **drink tonight** helper.

**Demo:** https://mascot27.github.io/vino-della-casa/

## Stack

- .NET 8 / Blazor WASM
- Domain + application layers, xUnit tests, GitHub Actions CI
- Offline path: `ICellarStore` + IndexedDB (in-memory store for CI)

## Run locally

```bash
dotnet test
dotnet run --project src/VinoDellaCasa.Web
```

Open the URL shown by `dotnet run`. On GitHub Pages, open the demo link above (project site base path `/vino-della-casa/`).

## Docs

- `Docs/architecture.md` — IndexedDB vs test host
- `Docs/SPEC.md` — product & engineering notes
- `Docs/domain/` — maturity rules & sample seed notes

## Dev hygiene

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet format
```

CI collects Coverlet cobertura results as a workflow artifact (no coverage threshold yet). `dotnet format --verify-no-changes` runs as a soft check (`continue-on-error`).

