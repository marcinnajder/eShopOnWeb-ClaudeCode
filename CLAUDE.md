# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Requires .NET SDK 10.0.4xx (`global.json`). Run from the repository root.

```bash
dotnet build eShopOnWeb.slnx
dotnet test tests/UnitTests/UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests.csproj
dotnet test tests/FunctionalTests/FunctionalTests.csproj
dotnet test tests/PublicApiIntegrationTests/PublicApiIntegrationTests.csproj
dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~BasketAddItem"   # single test/class
dotnet format eShopOnWeb.slnx --verify-no-changes
```

- Test projects use xUnit v3, except `PublicApiIntegrationTests` (MSTest). All of them run on in-memory databases.
- `tests/EndToEndTests` (Playwright) needs browsers installed first (see its README); skip it otherwise. `dotnet ./BuildTestFormat.cs` runs restore, build, all `*Tests` projects (including E2E) and format check.

## Running the app

The app is two processes; both are needed for the admin panel:

```bash
dotnet run --project src/Web --launch-profile https        # https://localhost:5001 (store + /admin)
dotnet run --project src/PublicApi --launch-profile https  # https://localhost:5099/swagger
```

- Start **Web first**: in the `Development` environment both apps share SQLite files in `.data/` (`UseSqliteDatabase` in `appsettings.Development.json`), and only Web creates and seeds the database; PublicApi only connects. Delete `.data/` to reset data.
- Database mode (`src/Infrastructure/Dependencies.cs`, `ConfigureLocalDatabaseContexts`): `UseSqliteDatabase` → SQLite, else `UseOnlyInMemoryDatabase` → per-process in-memory DB, else SQL Server (Docker/production). Override with environment variables.
- The `https` launch profile sets `ASPNETCORE_ENVIRONMENT=Development`; `dotnet run --environment X` does not (in .NET 10 it is an alias for `-e`).
- Demo users (password = `AuthorizationConstants.DEFAULT_PASSWORD`): `demouser@microsoft.com`, `productmgr@microsoft.com` (role Product Managers – catalog CRUD in `/admin`), `admin@microsoft.com` (role Administrators – users and roles only; catalog endpoints return 403).

## Architecture

Clean Architecture monolith (reference app for the "Architecting Modern Web Applications with ASP.NET Core and Azure" eBook):

- **ApplicationCore** – domain, no infrastructure dependencies: entities and aggregates (`Basket`, `Order`, `Buyer`, `CatalogItem`), services (`BasketService`, `OrderService`), interfaces `IRepository<T>` / `IReadRepository<T>` (Ardalis.Specification, aggregate roots only) and query specifications in `Specifications/`.
- **Infrastructure** – EF Core: `CatalogContext` (catalog, baskets, orders) and `AppIdentityDbContext` (ASP.NET Identity) as two separate contexts/databases, generic `EfRepository<T>`, entity configs in `Data/Config`, seeding in `CatalogContextSeed` / `AppIdentityDbContextSeed`. SQL Server migrations live in `Data/Migrations` and `Identity/Migrations`; SQLite uses `EnsureCreated()` instead, and HiLo sequences are stripped from the model on SQLite (`CatalogContext.OnModelCreating`).
- **Web** – the store: server-rendered Razor Pages + MVC controllers with cookie authentication; some features use the Mediator pattern (`Features/MyOrders`, `Features/OrderDetails`). Web also hosts the admin SPA files.
- **BlazorAdmin** – admin panel as Blazor WebAssembly running in the browser. It cannot access the database, so all data goes through PublicApi over HTTP with a JWT obtained from Web (`UserController`) – see `Services/HttpService.cs`.
- **PublicApi** – REST API built with FastEndpoints (one class per endpoint, grouped by feature folder), JWT bearer auth with role checks, CORS for the admin SPA, Swagger.
- **BlazorShared** – DTOs and role constants shared by BlazorAdmin, PublicApi and ApplicationCore.
- **eShopWeb.AppHost / eShopWeb.AspireServiceDefaults** – optional .NET Aspire orchestration (Web, PublicApi, Seq) and OpenTelemetry defaults.

Package versions are managed centrally in `Directory.Packages.props`.

## Conventions

- English for everything that lands in the repo or on GitHub: code, comments, commit messages, branch names, issues, PRs and review comments. Chat with the user may be in Polish.
- Task branch: `feature/<issue-number>-<short-slug>` (e.g. `feature/12-product-search`), created from `main` and linked to the issue (`gh issue develop`).
- Issue body has two sections: `Requirements` and `Implementation plan`.
- PR title starts with the issue number (e.g. `#12 Add product search`); the description contains a summary of changes, how to test, and `Closes #<issue-number>`.
- Before committing, the build and tests must pass.
- Never commit, push, or publish anything on GitHub (issues, PRs, comments) unless the user asks for it.
