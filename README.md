# Vino della casa

Local-first Blazor WebAssembly wine cellar: track stock and bin locations, search your bottles, and get a deterministic **drink tonight** shortlist.

**Live demo:** https://mascot27.github.io/vino-della-casa/

## Screenshots

Accueil:

![Accueil](Docs/screenshots/home-hero.png)

Cellar — bottle cards:

![Cellar — bottle cards](Docs/screenshots/cellar-cards.png)

Drink tonight — ranked ready bottles:

![Drink tonight — ranked ready bottles](Docs/screenshots/drink-tonight.png)

## Photo credits

Home hero and feature accents use local Unsplash photos (Unsplash License): Bob Brewer, Jennifer Yung, Gonzalo Facello — see `wwwroot/img/hero/ATTRIBUTION.md`.

## What it does

- **Cave** — browse InStock bottles as cards (color, vintage, location)
- **Search & filters** — find bottles by varietal, region, status
- **Drink tonight** — rank bottles that are ready to drink from your cellar
- **Offline-friendly** — local store path (IndexedDB in the browser; in-memory for CI)

## Stack

- .NET 8 / Blazor WASM
- Domain + application layers, xUnit tests, GitHub Actions CI

## Run locally

```bash
dotnet test
dotnet run --project src/VinoDellaCasa.Web
```

Open the URL printed by `dotnet run`. On GitHub Pages, use the demo link above (site base path `/vino-della-casa/`).

## Docs

- `Docs/architecture.md` — IndexedDB vs test host
- `Docs/SPEC.md` — product & engineering notes
- `Docs/domain/` — maturity rules, sample seeds, MCP tool contract

## Local MCP server (Cursor / agents)

Read-only stdio tools over domain logic (`evaluate_maturity`, `rank_drink_tonight`, `list_seed_bottles`, …). Contract: `Docs/domain/MCP-TOOLS-CONTRACT.md`.

```bash
dotnet run --project src/VinoDellaCasa.Mcp
```

Example Cursor MCP config (no secrets — replace the absolute path):

```json
{
  "mcpServers": {
    "vino-della-casa": {
      "command": "dotnet",
      "args": ["run", "--project", "/ABS/PATH/TO/vino-della-casa/src/VinoDellaCasa.Mcp"]
    }
  }
}
```

Smoke: `dotnet test tests/VinoDellaCasa.Mcp.Tests`. Stdio only; no network writes.

## Dev hygiene

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet format
```

CI collects Coverlet cobertura results as a workflow artifact (no coverage threshold yet). `dotnet format --verify-no-changes` runs as a soft check (`continue-on-error`).
