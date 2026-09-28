# Development

## Prerequisites

- .NET 9 SDK
- Docker Desktop
- PowerShell 7 or another compatible shell

## Build

```powershell
dotnet restore BoutiqueEnLigne.sln
dotnet build BoutiqueEnLigne.sln
```

## Tests

Run unit and architecture tests locally:

```powershell
dotnet test BoutiqueEnLigne.sln --no-build
```

The suite contains fast Domain tests, dependency-rule architecture tests and SQLite-backed integration tests for Ordering's transactional Outbox and idempotent Inbox. The CI workflow restores, builds and executes the same suite on .NET 9 for every pull request and every push to the default branches.

## Run with Docker

Copy `.env.example` to `.env`, replace the placeholder values, then run:

```powershell
docker compose up --build
```

## Database migrations

Identity, Catalog, Ordering and Payments own separate EF Core migration histories. Their APIs apply pending migrations during startup with `Database.MigrateAsync()`; production deployments should run the same migrations as a controlled deployment step before scaling application replicas.

Create a new migration in the service that owns the changed model:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Services/<Service>/BoutiqueEnLigne.<Service>.Infrastructure `
  --startup-project src/Services/<Service>/BoutiqueEnLigne.<Service>.Api `
  --context <Service>DbContext `
  --output-dir Persistence/Migrations
```

Never share a migration or a database schema between services. Review generated SQL before applying a migration to production:

```powershell
dotnet ef migrations script --idempotent `
  --project src/Services/<Service>/BoutiqueEnLigne.<Service>.Infrastructure `
  --startup-project src/Services/<Service>/BoutiqueEnLigne.<Service>.Api
```

Databases created by an older build with `EnsureCreatedAsync()` do not contain EF's migration-history table. Back them up and establish a migration baseline (or recreate development-only volumes) before starting this version; do not point the initial migrations at an existing production schema without that preparation.
