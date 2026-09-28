# BoutiqueEnLigne

BoutiqueEnLigne est une plateforme e-commerce conçue comme projet de référence pour une architecture microservices distribuée avec ASP.NET Core, Ocelot, RabbitMQ, SQL Server, Redis, Stripe, Docker et OpenTelemetry.

Le commerce en ligne est le domaine fonctionnel utilisé pour démontrer des pratiques réelles : services autonomes, base de données par service, communication asynchrone fiable, sécurité JWT, migrations versionnées, observabilité et tests automatisés.

## Architecture

```text
Client
  -> BoutiqueEnLigne.Web
  -> BoutiqueEnLigne.Gateway
     -> Identity API
     -> Catalog API
     -> Cart API
     -> Ordering API
     -> Payments API

Ordering -- Outbox --> RabbitMQ --> Inbox -- Payments events --> Ordering
```

Chaque service backend est une application ASP.NET Core déployable indépendamment et organisée en quatre couches : `Api`, `Application`, `Domain` et `Infrastructure`. Aucun service ne partage directement sa base de données avec un autre.

Le traitement des paiements repose sur une livraison « at least once » : les événements sont écrits dans un Outbox transactionnel, publiés vers RabbitMQ, puis dédupliqués par l’Inbox du consommateur. Les événements impossibles à traiter sont dirigés vers une Dead Letter Queue.

## Repository structure

```text
BoutiqueEnLigne/
├── .github/workflows/                CI workflows
├── deploy/docker/                    Container build assets
├── docs/                             Architecture and development documentation
├── src/
│   ├── BuildingBlocks/
│   │   ├── BoutiqueEnLigne.Contracts/
│   │   ├── BoutiqueEnLigne.EventBus/
│   │   └── BoutiqueEnLigne.Observability/
│   ├── Gateway/
│   │   └── BoutiqueEnLigne.Gateway/
│   ├── Services/
│   │   ├── Identity/
│   │   │   ├── BoutiqueEnLigne.Identity.Api/
│   │   │   ├── BoutiqueEnLigne.Identity.Application/
│   │   │   ├── BoutiqueEnLigne.Identity.Domain/
│   │   │   └── BoutiqueEnLigne.Identity.Infrastructure/
│   │   ├── Catalog/
│   │   │   ├── BoutiqueEnLigne.Catalog.Api/
│   │   │   ├── BoutiqueEnLigne.Catalog.Application/
│   │   │   ├── BoutiqueEnLigne.Catalog.Domain/
│   │   │   └── BoutiqueEnLigne.Catalog.Infrastructure/
│   │   ├── Cart/
│   │   │   ├── BoutiqueEnLigne.Cart.Api/
│   │   │   ├── BoutiqueEnLigne.Cart.Application/
│   │   │   ├── BoutiqueEnLigne.Cart.Domain/
│   │   │   └── BoutiqueEnLigne.Cart.Infrastructure/
│   │   ├── Ordering/
│   │   │   ├── BoutiqueEnLigne.Ordering.Api/
│   │   │   ├── BoutiqueEnLigne.Ordering.Application/
│   │   │   ├── BoutiqueEnLigne.Ordering.Domain/
│   │   │   └── BoutiqueEnLigne.Ordering.Infrastructure/
│   │   └── Payments/
│   │       ├── BoutiqueEnLigne.Payments.Api/
│   │       ├── BoutiqueEnLigne.Payments.Application/
│   │       ├── BoutiqueEnLigne.Payments.Domain/
│   │       └── BoutiqueEnLigne.Payments.Infrastructure/
│   └── Web/
│       └── BoutiqueEnLigne.Web/
├── tests/
│   ├── Architecture/
│   ├── Integration/
│   └── Unit/
├── BoutiqueEnLigne.sln
└── docker-compose.yml
```

## Composants

| Component | Responsibility | Local URL |
|---|---|---|
| Web | MVC user interface | `http://localhost:5212` |
| Gateway | Single backend entry point and routing | `http://localhost:5000` |
| Identity | Accounts, authentication and sellers | `http://localhost:5001` |
| Catalog | Products and inventory | `http://localhost:5002` |
| Ordering | Order lifecycle | `http://localhost:5003` |
| Payments | Stripe payment integration | `http://localhost:5004` |
| Cart | Shopping-cart lifecycle | `http://localhost:5005` |

Infrastructure locale :

| Component | Responsibility | Local endpoint |
|---|---|---|
| SQL Server | Databases owned by SQL-backed services | `localhost:1433` |
| Redis | Cart persistence | `localhost:6379` |
| RabbitMQ | Integration-event broker | `localhost:5672` |
| RabbitMQ Management | Broker administration | `http://localhost:15672` |
| OpenTelemetry Collector | OTLP traces and metrics | `localhost:4317` / `4318` |

## Prérequis

- .NET 9 SDK et runtime
- Docker Desktop avec Docker Compose
- PowerShell 7, Bash ou un terminal équivalent

## Compiler et tester

```powershell
dotnet restore BoutiqueEnLigne.sln
dotnet build BoutiqueEnLigne.sln
dotnet test BoutiqueEnLigne.sln --no-build
```

La suite contient des tests unitaires, des tests d’architecture et des tests d’intégration SQLite pour les transactions Outbox/Inbox.

## Démarrer avec Docker

Créer le fichier d’environnement local :

```powershell
Copy-Item .env.example .env
```

Remplacer toutes les valeurs `CHANGE_ME_*` dans `.env`, puis démarrer la plateforme :

```powershell
docker compose up --build
```

## Sécurité et fiabilité

- mots de passe hachés et jamais retournés par les API ;
- JWT courts et refresh tokens rotatifs stockés sous forme de hash ;
- contrôle d’accès par identité sur Cart, Ordering et Payments ;
- validation cryptographique des webhooks Stripe ;
- événements versionnés, messages persistants et publisher confirms RabbitMQ ;
- Outbox transactionnel, Inbox idempotent et Dead Letter Queue ;
- migrations EF Core indépendantes pour chaque base SQL.

## Observabilité

Ordering et Payments exposent `/health/live` et `/health/ready`, propagent `X-Correlation-ID` et exportent traces et métriques via OTLP. Le Building Block partagé permet d’étendre la même configuration aux autres services.

## État actuel

L’architecture est estimée à environ **92 %** de la cible prévue. Les principaux travaux restants sont l’activation de l’observabilité sur tous les exécutables, les health checks des dépendances externes et l’extension des tests d’intégration aux API complètes.

Consulter [la vue d’architecture](docs/architecture/overview.md) et [le guide de développement](docs/development.md) pour plus de détails.
