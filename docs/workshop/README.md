# Battle Ops — Aspire Workshop

> A hands-on workshop series that uses the **Battle Ops** Battleship game to teach .NET Aspire orchestration, service discovery, observability, and distributed-application patterns.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js LTS](https://nodejs.org/) with npm 11+
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) (optional — Aspire manages the sidecar)
- A code editor (Visual Studio 2022 17.14+, VS Code, or Rider)

---

## Modules

| # | Module | Level | Duration | Topics |
|---|--------|-------|----------|--------|
| 01 | [Dashboard](./01-dashboard.md) | Beginner | 45–60 min | Aspire basics, AppHost anatomy, first run, dashboard navigation |
| 02 | [Built-in Integrations](./02-built-in-integrations.md) | Beginner–Intermediate | 45–60 min | Hosting vs client packages, PostgreSQL, queues, Keycloak |
| 03 | [Service Defaults](./03-service-defaults.md) | Intermediate | 30–45 min | OpenTelemetry, health checks, Polly resilience |
| 04 | [Resource Orchestration](./04-resource-orchestration.md) | Intermediate | 30–45 min | WithReference, WaitFor, migrations pattern, volumes |
| 05 | [Lifecycle Hooks](./05-lifecycle-hooks.md) | Intermediate–Advanced | 45–60 min | IDistributedApplicationLifecycleHook, data seeding, annotations |
| 06 | [Custom Resource Types](./06-custom-resource-types.md) | Advanced | 45–60 min | Resource abstractions, IResourceBuilder extensions, NuGet publishing |
| 07 | [Automated Testing](./07-automated-testing.md) | Intermediate–Advanced | 60–75 min | DistributedApplicationTestingBuilder, test isolation, Respawn |
| 08 | [Dashboard Deep Dive](./08-dashboard-deep-dive.md) | Intermediate | 30–45 min | OTLP export, PgWeb pattern, standalone dashboard |

---

## How to use this workshop

1. Clone the repository and ensure all prerequisites are installed.
2. Work through the modules in order — each builds on concepts from the previous one.
3. Every module includes a **Hands-on** section with step-by-step exercises.
4. Use the Aspire Dashboard as your primary observability tool throughout.

---

## About the sample application

**Battle Ops** is a real-time, two-player Battleship game that runs as an Aspire-orchestrated distributed application. It demonstrates:

- **Modular monolith with CQRS** — isolated domain modules composed through DI
- **Event-driven real-time pipeline** — Dapr pub/sub → SignalR → WebSocket
- **Anonymous player sessions** — JWT-based auth with Dapr state store persistence
- **OpenTelemetry observability** — traces, metrics, and structured logs across all domains

The game serves as a realistic, engaging example that exercises the full Aspire feature set.
