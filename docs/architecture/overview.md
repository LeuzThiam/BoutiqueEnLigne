# Architecture overview

BoutiqueEnLigne is an e-commerce system used to demonstrate a distributed microservice architecture.

The ASP.NET Core MVC web application communicates with backend services through an Ocelot API Gateway. Each business capability is isolated in a separately deployable service:

- Identity: accounts, authentication and roles.
- Catalog: products, categories and inventory.
- Cart: shopping-cart lifecycle.
- Ordering: order creation and lifecycle.
- Payments: Stripe integration and payment lifecycle.

The target architecture applies database-per-service, asynchronous integration events, independent deployments, distributed tracing and service-level tests. These capabilities will be introduced incrementally while keeping the solution buildable.

## Asynchronous messaging

Shared integration contracts live in `src/BuildingBlocks/BoutiqueEnLigne.Contracts`. RabbitMQ transport code is isolated in `BoutiqueEnLigne.EventBus`; business services therefore do not depend directly on the broker client.

Ordering uses the transactional Outbox pattern. Creating an order stores both the order and an `ordering.order-created.v1` message in `OrderingDb` in the same SQL transaction. A background processor publishes pending messages to the durable `boutique.events` topic exchange with publisher confirmations. A broker outage does not lose the event: it remains in the Outbox for a later retry.

Payments applies the same pattern after validating Stripe webhook signatures. The payment status, Stripe webhook receipt and either `payments.payment-succeeded.v1` or `payments.payment-failed.v1` are committed atomically in `PaymentsDb`. The checkout creates a Pending order first, then includes its `OrderId` in the Stripe PaymentIntent metadata, allowing downstream consumers to correlate payment results without sharing databases.

```text
Ordering API -> SQL transaction [Order + OutboxMessage]
                               -> Outbox worker -> RabbitMQ topic exchange
```

Ordering consumes payment results from the durable `ordering.payments.v1` queue. It updates the order and records the event identifier in `InboxMessages` in one SQL transaction, providing idempotent at-least-once processing. Successful payments confirm the order; failed payments leave it Pending so another payment attempt remains possible. Malformed or unprocessable events are dead-lettered to `ordering.payments.dead-letter.v1` for diagnosis instead of blocking the main queue.

Integration-event contracts are versioned in their routing names so that schemas can evolve without silently breaking consumers.

## Database evolution

Every SQL-backed service owns its EF Core migrations alongside its Infrastructure persistence code. APIs apply pending migrations with bounded startup retries. Migration histories remain isolated per database, preserving database-per-service ownership and allowing each service to evolve independently.

## Automated verification

Unit tests protect payment and order state transitions. Architecture tests prevent Domain assemblies from acquiring framework or infrastructure dependencies. SQLite-backed integration tests execute real transactions to verify that order creation writes its Outbox record and that duplicate payment deliveries produce exactly one Inbox receipt.

## Observability

The shared `BoutiqueEnLigne.Observability` building block standardizes W3C-compatible distributed tracing, HTTP and runtime metrics, health endpoints and `X-Correlation-ID` propagation. Ordering and Payments export OTLP telemetry to the OpenTelemetry Collector. `/health/live` reports process liveness while `/health/ready` is the readiness contract for orchestrators. Health traffic is excluded from request traces to avoid telemetry noise.

## Request flow

```text
Client -> Web -> Gateway -> Identity | Catalog | Cart | Ordering | Payments
```

The Gateway contains routing concerns only. Business logic remains inside the owning service.

## Service boundaries

Services are migrated incrementally to four projects with inward-facing dependencies:

```text
Api -> Application -> Domain
Api -> Infrastructure -> Application
Infrastructure -> Domain
```

`Domain` contains business entities and rules without ASP.NET Core or Entity Framework dependencies. `Application` defines use cases and abstractions. `Infrastructure` implements persistence and external integrations. `Api` is the HTTP composition root.

Catalog, Identity, Cart, Ordering and Payments have been migrated to this structure. Identity stores password hashes only and never exposes them through API response contracts. Cart stores authenticated user baskets in Redis with sliding expiration. Ordering and Payments persist their data in isolated SQL databases. Stripe webhooks are signature-validated and recorded to prevent duplicate processing.

Identity issues short-lived signed JWT access tokens and rotating refresh tokens. Refresh tokens are stored as SHA-256 hashes, can be revoked, and are replaced whenever they are used. The Web application keeps tokens in its server-side session and attaches the access token to outgoing API requests.
