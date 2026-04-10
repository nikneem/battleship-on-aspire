# Module 02 — Built-in Integrations

> **Level:** Beginner–Intermediate · **Duration:** 45–60 min

## Learning objectives

- Understand the difference between hosting packages and client packages
- Add a database integration (PostgreSQL)
- Add a messaging integration (Azure Storage Queues)
- Add an identity integration (Keycloak)
- Know how Aspire manages container lifecycle for integrations

---

## Hosting packages vs client packages

Aspire integrations come in two flavors:

| Package type | NuGet prefix | Where it's referenced | What it does |
|---|---|---|---|
| **Hosting** | `Aspire.Hosting.*` | AppHost project | Declares the resource (spins up a container, provisions a cloud resource) |
| **Client** | `Aspire.*` | Service project | Configures the client SDK (connection pooling, health checks, telemetry) |

Example for PostgreSQL:

```
AppHost         → Aspire.Hosting.PostgreSQL      (creates the container)
API project     → Aspire.Npgsql.EntityFrameworkCore (configures EF Core)
```

The hosting package creates the infrastructure. The client package wires up your code to use it.

---

## Database integration — PostgreSQL

### Step 1: Add to the AppHost

```csharp
var postgres = builder.AddPostgres("pg")
    .WithPgAdmin();            // optional: adds pgAdmin container

var gameDb = postgres.AddDatabase("gamedb");
```

### Step 2: Reference from the API

```csharp
var api = builder.AddProject<Projects.HexMaster_BattleShip_Api>("battleship-api")
    .WithReference(gameDb)     // injects ConnectionStrings__gamedb
    .WaitFor(postgres);        // waits for PostgreSQL to be healthy
```

### Step 3: Use in the API project

```csharp
// In Program.cs
builder.AddNpgsqlDbContext<GameDbContext>("gamedb");
```

Aspire automatically:
- Pulls and runs the PostgreSQL container
- Generates a connection string
- Injects it via environment variables
- Adds health checks and retry policies

---

## Messaging integration — Azure Storage Queues

### Step 1: Add to the AppHost

```csharp
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();          // uses Azurite in development

var queues = storage.AddQueues("queues");
```

### Step 2: Reference and use

```csharp
var api = builder.AddProject<Projects.HexMaster_BattleShip_Api>("battleship-api")
    .WithReference(queues);

// In Program.cs of the API
builder.AddAzureQueueClient("queues");
```

---

## Identity integration — Keycloak

### Step 1: Add to the AppHost

```csharp
var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithDataVolume();         // persist config across restarts

var realm = keycloak.AddRealm("battleship");
```

### Step 2: Reference and use

```csharp
var api = builder.AddProject<Projects.HexMaster_BattleShip_Api>("battleship-api")
    .WithReference(realm);

// In Program.cs of the API
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer("keycloak", realm: "battleship");
```

---

## Hands-on exercise

1. **Add PostgreSQL** to the Battle Ops AppHost
   - Install `Aspire.Hosting.PostgreSQL` in the AppHost project
   - Declare a `postgres` resource and a `gamedb` database
   - Reference it from the API project
   - Run the app and observe the PostgreSQL container in the Dashboard

2. **Add Azure Storage Queues** using the emulator
   - Install `Aspire.Hosting.Azure.Storage` in the AppHost
   - Add storage with `.RunAsEmulator()` and add queues
   - Reference from the API
   - Verify the Azurite container appears in the Dashboard

3. **Explore resource health**
   - In the Dashboard, observe the health status of each integration
   - Try stopping the PostgreSQL container manually (via Docker) and watch the health checks react

---

## Key takeaways

- Hosting packages live in the AppHost; client packages live in service projects
- `WithReference()` automatically injects connection information
- `.RunAsEmulator()` lets you develop locally without cloud dependencies
- Aspire manages the full container lifecycle: pull, start, health check, connection string injection
- `WaitFor()` ensures dependent services start only after their dependencies are healthy
