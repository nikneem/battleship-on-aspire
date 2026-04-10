# Module 01 — Dashboard

> **Level:** Beginner · **Duration:** 45–60 min

## Learning objectives

- Understand what .NET Aspire solves: orchestration, service discovery, and observability
- Know the anatomy of an AppHost project
- Run your first Aspire application
- Navigate the Aspire Dashboard

---

## What is .NET Aspire?

.NET Aspire is an opinionated stack for building observable, production-ready, distributed applications. It provides:

| Capability | What it does |
|---|---|
| **Orchestration** | Declares resources (projects, containers, executables) and their dependencies in C# |
| **Service discovery** | Automatically wires connection strings and endpoint references between resources |
| **Observability** | Captures OpenTelemetry logs, traces, and metrics from every resource out of the box |
| **Developer dashboard** | A local web UI that surfaces all telemetry, resource status, and endpoints in one place |

---

## AppHost anatomy

The AppHost is a small .NET project that describes your distributed application. Open `src/Aspire/HexMaster.BattleShip.Aspire.AppHost/AppHost.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var stateStore = builder.AddDaprStateStore("statestore");
var pubSub     = builder.AddDaprPubSub("pubsub");

// API project with Dapr sidecar
var api = builder.AddProject<Projects.HexMaster_BattleShip_Api>("battleship-api")
    .WithDaprSidecar()
    .WithReference(stateStore)
    .WithReference(pubSub);

// Angular frontend
builder.AddNpmApp("battleship", "../../../App")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

Key concepts:
- **Resources** — anything Aspire manages: projects, containers, executables, npm apps
- **References** — `WithReference()` injects connection information (URLs, connection strings) into the target
- **Dapr sidecar** — `WithDaprSidecar()` attaches a Dapr runtime to a project resource

---

## Running the application

```bash
# From the repository root
dotnet workload restore
dotnet run --project src/Aspire/HexMaster.BattleShip.Aspire.AppHost
```

The Aspire Dashboard opens automatically in your browser.

---

## Exploring the Dashboard

### Resources tab
Shows every resource declared in the AppHost:
- **Name** — the resource identifier
- **Type** — project, container, or executable
- **State** — Running, Starting, Waiting, etc.
- **Endpoints** — clickable URLs for HTTP resources

### Structured logs
- View logs from all resources in one place
- Filter by resource, severity, or text search
- Structured fields (key-value pairs) are preserved

### Traces
- Distributed traces that span multiple resources
- Click a trace to see the waterfall view
- Each span shows timing, tags, and status

### Metrics
- Counters, histograms, and gauges from all resources
- OpenTelemetry metrics are collected automatically via ServiceDefaults

---

## Hands-on exercise

1. **Start the application** using the command above
2. **Open the Dashboard** — note the URL (usually `https://localhost:17222`)
3. **Find the API resource** — click its endpoint to open the API in your browser
4. **Find the Angular app** — click its endpoint to open Battle Ops
5. **Explore structured logs** — create an anonymous session by visiting the app, then find the corresponding log entries
6. **Explore traces** — the session creation should produce a distributed trace; find it and inspect the spans
7. **Stop the application** — press `Ctrl+C` in the terminal and observe the shutdown sequence in the Dashboard

---

## Key takeaways

- The AppHost is the single place where your distributed topology is defined
- Resources, references, and sidecars are composed fluently in C#
- The Dashboard gives you instant observability without any extra configuration
- All telemetry (logs, traces, metrics) flows through OpenTelemetry and is displayed automatically
