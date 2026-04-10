# Module 03 — Service Defaults

> **Level:** Intermediate · **Duration:** 30–45 min

## Learning objectives

- Understand what ServiceDefaults provides out of the box
- Configure OpenTelemetry: logs, metrics, and traces
- Set up health checks
- Add resilience with Polly
- Understand why ServiceDefaults matters beyond development

---

## What are Service Defaults?

The `ServiceDefaults` project is a shared library that every service project references. It provides a consistent set of cross-cutting concerns:

```
src/Aspire/HexMaster.BattleShip.Aspire.ServiceDefaults/
└── Extensions.cs
```

When a service calls `builder.AddServiceDefaults()`, it gets:

| Feature | What it does |
|---|---|
| **OpenTelemetry** | Exports logs, traces, and metrics to the Aspire Dashboard (and any configured OTLP endpoint) |
| **Health checks** | Registers `/health` and `/alive` endpoints |
| **Service discovery** | Enables `http://resource-name` URL resolution |
| **Resilience** | Adds Polly retry and circuit-breaker policies to HttpClient |

---

## OpenTelemetry configuration

ServiceDefaults configures the full OpenTelemetry pipeline:

### Logging
```csharp
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

### Tracing
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddSource("HexMaster.BattleShip.Games")
               .AddSource("HexMaster.BattleShip.Profiles")
               .AddSource("HexMaster.BattleShip.Realtime");
    });
```

Custom `ActivitySource` names (like `HexMaster.BattleShip.Games`) are registered so that domain-specific traces appear in the Dashboard.

### Metrics
```csharp
.WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddRuntimeInstrumentation()
           .AddMeter("HexMaster.BattleShip.Games")
           .AddMeter("HexMaster.BattleShip.Profiles")
           .AddMeter("HexMaster.BattleShip.Realtime");
});
```

---

## Health checks

ServiceDefaults registers two health check endpoints:

| Endpoint | Purpose |
|---|---|
| `/health` | Full health check — includes all registered checks |
| `/alive` | Liveness probe — lightweight, always returns healthy if the process is running |

You can add custom health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => /* check database connectivity */);
```

---

## Resilience with Polly

ServiceDefaults adds a standard resilience pipeline to all `HttpClient` instances:

```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
});
```

This includes:
- **Retry** — exponential backoff for transient failures
- **Circuit breaker** — stops calling a failing service after repeated failures
- **Timeout** — prevents indefinite hangs

---

## Why this matters beyond development

ServiceDefaults is not just a dev convenience. The same configuration works in production:

- OpenTelemetry exports to any OTLP-compatible backend (Azure Monitor, Jaeger, Prometheus)
- Health checks integrate with Kubernetes liveness/readiness probes
- Polly resilience protects against real network failures
- Service discovery resolves to actual DNS names in deployment

---

## Hands-on exercise

1. **Open ServiceDefaults** at `src/Aspire/HexMaster.BattleShip.Aspire.ServiceDefaults/Extensions.cs`

2. **Customize OpenTelemetry**
   - Add a custom `ActivitySource` for a new domain
   - Run the app and verify your custom traces appear in the Dashboard

3. **Add a custom health check**
   - Register a health check that verifies a condition (e.g., checking that a file exists or a config value is set)
   - Run the app and navigate to `/health` on the API to see the result

4. **Observe telemetry in the Dashboard**
   - Play a game (create + join + fire shots)
   - In the Dashboard, find the distributed traces for a "fire shot" operation
   - Examine the span tree — you should see the HTTP request, the command handler, and the Dapr pub/sub publish

5. **Test resilience**
   - Add a deliberate delay or failure to an HTTP call
   - Observe Polly retries in the traces

---

## Key takeaways

- ServiceDefaults is the single configuration point for cross-cutting concerns
- OpenTelemetry gives you logs + traces + metrics with no per-service configuration
- Health checks and resilience work identically in development and production
- Custom `ActivitySource` and `Meter` registrations connect domain telemetry to the pipeline
