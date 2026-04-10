# Module 08 — Dashboard Deep Dive

> **Level:** Intermediate · **Duration:** 30–45 min

## Learning objectives

- Explore advanced Dashboard capabilities: logs, traces, metrics, and resource management
- Add custom URL annotations with `WithUrlForEndpoint`
- Configure OTLP export for external observability backends
- Add companion tools (PgWeb pattern)
- Deploy the Dashboard standalone for production use

---

## Dashboard capabilities

The Aspire Dashboard is more than a development convenience — it's a full observability frontend built on OpenTelemetry.

### Structured logs

- Filter by resource, severity, timestamp, or text
- Structured fields are displayed as key-value pairs
- Supports OpenTelemetry log semantic conventions

### Distributed traces

- Waterfall view shows span timing across services
- Click any span to see tags, status, and events
- Filter by resource, duration, or error status
- Traces flow from HTTP request → command handler → Dapr publish → SignalR broadcast

### Metrics

- Counters, histograms, and gauges from all resources
- Built-in metrics: HTTP request duration, active connections, error rates
- Custom metrics from domain telemetry classes (e.g., `battleship.games.shots.fired`)

### Resource management

- Start, stop, and restart individual resources
- View environment variables and endpoints
- Monitor health check status in real-time

---

## Custom URL annotations

Add custom links to resources in the Dashboard:

```csharp
var api = builder.AddProject<Projects.Api>("battleship-api")
    .WithUrlForEndpoint("swagger", url => $"{url}/swagger",
        displayText: "Swagger UI",
        displayLocation: ResourceDisplayLocation.Details);
```

This adds a clickable "Swagger UI" link in the Dashboard for the API resource.

You can add multiple URLs:

```csharp
var api = builder.AddProject<Projects.Api>("battleship-api")
    .WithUrlForEndpoint("health", url => $"{url}/health",
        displayText: "Health Check")
    .WithUrlForEndpoint("metrics", url => $"{url}/metrics",
        displayText: "Prometheus Metrics");
```

---

## OTLP export configuration

By default, telemetry flows to the local Dashboard. To also export to external backends:

### Azure Monitor / Application Insights

```csharp
// In ServiceDefaults
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration
            .GetConnectionString("ApplicationInsights");
    });
```

### Generic OTLP endpoint (Jaeger, Grafana, etc.)

```csharp
// Set via environment variable
OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

Or in code:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddOtlpExporter(o =>
    {
        o.Endpoint = new Uri("http://jaeger:4317");
        o.Protocol = OtlpExportProtocol.Grpc;
    }));
```

---

## Companion tools — PgWeb pattern

Add database management tools alongside your databases:

### PgWeb (PostgreSQL web client)

```csharp
var postgres = builder.AddPostgres("pg")
    .WithPgAdmin();  // built-in pgAdmin support

// Or add PgWeb as a custom container
builder.AddContainer("pgweb", "sosedoff/pgweb")
    .WithHttpEndpoint(port: 8081, targetPort: 8081)
    .WithEnvironment("PGWEB_DATABASE_URL",
        postgres.Resource.ConnectionStringExpression)
    .WaitFor(postgres);
```

### Redis Commander

```csharp
var redis = builder.AddRedis("cache");

builder.AddContainer("redis-commander", "rediscommander/redis-commander")
    .WithHttpEndpoint(port: 8082, targetPort: 8081)
    .WithEnvironment("REDIS_HOSTS", "local:cache:6379")
    .WaitFor(redis);
```

These companion tools appear in the Dashboard and are accessible via their endpoints.

---

## Standalone Dashboard deployment

The Aspire Dashboard can run as a standalone container in production:

```bash
docker run -d \
  -p 18888:18888 \
  -p 4317:18889 \
  -e DASHBOARD__OTLP__AUTHMODE=ApiKey \
  -e DASHBOARD__OTLP__PRIMARYAPIKEY=your-api-key \
  mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

Configure your services to send OTLP data to the Dashboard:

```
OTEL_EXPORTER_OTLP_ENDPOINT=http://dashboard:4317
```

This gives you the same Dashboard experience in production without the AppHost.

---

## Hands-on exercise

1. **Add custom Dashboard URLs**
   - Add a Swagger UI link to the API resource
   - Add health check links for each service
   - Run the app and verify the links appear in the Dashboard

2. **Explore OTLP configuration**
   - Open ServiceDefaults and examine the OpenTelemetry configuration
   - Consider how you'd add an Azure Monitor exporter
   - What environment variables control OTLP export?

3. **Add a companion tool**
   - If PostgreSQL is configured, add PgWeb as a container resource
   - Verify it appears in the Dashboard with a clickable endpoint
   - Use it to inspect the database

4. **Deploy the standalone Dashboard** (conceptual)
   - Plan a deployment of the standalone Dashboard container
   - What configuration does it need?
   - How would your services connect to it?

---

## Key takeaways

- The Dashboard provides structured logs, distributed traces, and metrics in one place
- Custom URL annotations make related tools discoverable in the Dashboard
- OTLP export lets you send telemetry to any compatible backend
- Companion containers (PgWeb, Redis Commander) integrate seamlessly as Dashboard resources
- The standalone Dashboard works in production without the AppHost
