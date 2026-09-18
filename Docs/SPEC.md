# Vino della casa — Product & Engineering Spec (CV project)

| Field | Value |
|---|---|
| **Working title** | Vino della casa |
| **Tagline** | Local-first wine cellar — inventaire, casiers, “what to drink tonight” |
| **Stack (MVP)** | **.NET 8+ / Blazor WebAssembly** + **EF Core + SQLite** (PWA / offline-first) |
| **Status** | Public portfolio MVP |
| **Goal** | Clean public-ready portfolio piece aligned with a modern .NET / C# portfolio |
| **Non-goals (MVP)** | Social network, marketplace, scraped catalogues, required cloud account |

> **Pivot note (2026-09-18):** Prefer Blazor/C# over SwiftUI for CV fit. Swift/Hue remains a separate optional track.

---

## 1. Why this project (CV narrative)

Hiring managers should see:

1. **You ship** — runnable Blazor WASM app + clear README, not a dump of snippets.
2. **You model data** — inventory, bins, quantities, drinking windows.
3. **Modern .NET** — Blazor WASM, EF Core, SQLite, xUnit, GitHub Actions.
4. **Engineering hygiene** — layered code, tests, CI, docs, privacy.

README one-liner:

> Local-first Blazor WASM cellar app (EF Core + SQLite): stock, bin locations, search, and a deterministic “drink tonight” helper — portfolio project with tests and CI.

---

## 2. Product vision

**Vino della casa** = personal cellar manager: bottles you own, where they sit, qty left, ready to drink — **offline**, no account wall.

### User stories (MVP)

| ID | Want | So that |
|---|---|---|
| US1 | CRUD bottles (name, vintage, region, qty, price CHF, bin) | stock is accurate |
| US2 | Search / filter | find a bottle fast |
| US3 | Record drink / finish / move | qty stays true |
| US4 | “Drink tonight” shortlist | stop staring at shelves |
| US5 | CSV export / import | never lose the cellar |

### Out of scope v1

Vivino sync, social, required OCR, multi-user cloud, IAP.

---

## 3. Domain model (MVP)

Prefer a single `Bottle` entity (denormalized) for speed; normalize later if needed.

```
Bottle
  Id, Name, Producer, Region, Country
  Color (enum), Varietal?, Vintage?
  Quantity, PriceChf?, PurchaseDate?
  Bin?, Status (InStock | Drinking | Finished)
  Notes?, ReadyToDrink (bool)?
  CreatedAt, UpdatedAt
```

**Drink tonight** (pure, unit-tested): optional filters → sort `(ReadyNow DESC, Vintage ASC, Name)` → top N.

---

## 4. Technical architecture

### Stack

| Layer | Choice |
|---|---|
| UI | Blazor WASM + Bootstrap or minimal CSS |
| Data | `ICellarStore` + **IndexedDB** (browser offline); EF Core + SQLite for tests/CI host |
| Tests | xUnit + FluentAssertions (or Shouldly) |
| CI | GitHub Actions: `dotnet test` + build |

**Offline-first (CV lock, 2026-09-18):** Browser persistence via **`ICellarStore` + IndexedDB** (not localStorage — quota + fragile JSON). Highlight this capability in the README.

**EF Core + SQLite:** keep for domain/integration tests and an optional `dotnet run` host path so CI stays green and reviewers can run tests without a browser. Document both paths in `Docs/architecture.md` (WASM + IndexedDB primary; SQLite host for tests/CI).

If IndexedDB WASM interop proves painful, fall back to a documented Hybrid/desktop host — CV honesty > fake PWA.

### Folder layout

```
src/
  VinoDellaCasa.Domain/       # entities, enums, pure services
  VinoDellaCasa.Application/  # use cases, interfaces
  VinoDellaCasa.Infrastructure/ # EF Core, CSV, SQLite
  VinoDellaCasa.Web/          # Blazor WASM (or Hybrid) UI
tests/
  VinoDellaCasa.Domain.Tests/
  VinoDellaCasa.Application.Tests/
Docs/
  architecture.md
  csv.md
```

### Practices (implement + explain in README)

1. Domain has **no** Blazor / EF references  
2. Use cases own quantity changes (`Drink`, `Restock`, `Move`)  
3. DI everywhere; tests use in-memory / SQLite test DB  
4. Typed errors for CSV import  
5. CI on every PR  
6. No analytics / no secrets  
7. SemVer + CHANGELOG  
8. `.editorconfig` + `dotnet format` in CI (optional but good)

---

## 5. CSV

Columns: `name,producer,region,country,color,varietal,vintage,quantity,price_chf,bin,status,notes,updated_at`  
UTF-8, header required, skip bad rows with summary.

---

## 6. Milestones

| M | Deliverable |
|---|---|
| M0 | Solution + CI green + empty Blazor shell |
| M1 | CRUD + persistence |
| M2 | Search/filter + Drink tonight + domain tests |
| M3 | CSV import/export |
| M4 | Polish README/screenshots + tag `v0.1.0` |

---

## 7. Repo / git hygiene

- Keep the project **private** until Corentin says otherwise  
- **Git author/committer = Corentin Zeller** (his name + email on commits — - No secrets in the repository
- No third-party / employer IP
- Clean history suitable for a future public CV flip  

---

## 8. Naming

- Product: **Vino della casa**  
- Repo suggestion: `vino-della-casa`  
- Tone: home cellar, not luxury marketplace  

---

*Spec version: 0.2 — 2026-09-18 — Blazor/.NET pivot*  
