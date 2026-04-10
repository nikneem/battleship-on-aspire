# Module 04 — Resource Orchestration Patterns

> **Level:** Intermediate · **Duration:** 30–45 min

## Learning objectives

- Use `WithReference()`, `WaitFor()`, and `WaitForCompletion()` to declare resource dependencies
- Implement the migrations-as-a-service pattern
- Manage persistent vs transient containers
- Configure volume management and naming strategies
- Apply environment-aware configuration

---

## Resource dependencies

Aspire provides three mechanisms to express how resources depend on each other:

### `WithReference()`
Injects connection information from one resource into another:

```csharp
var db = builder.AddPostgres("pg").AddDatabase("gamedb");
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db);  // API gets ConnectionStrings__gamedb
```

### `WaitFor()`
Ensures a resource starts only after its dependency is healthy:

```csharp
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db)
    .WaitFor(db);  // API waits for PostgreSQL health check to pass
```

### `WaitForCompletion()`
Waits for a resource to run to completion before starting the dependent:

```csharp
var migration = builder.AddProject<Projects.Migrator>("migrator")
    .WithReference(db);

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db)
    .WaitForCompletion(migration);  // API starts only after migration exits
```

---

## Migrations-as-a-service pattern

Instead of running database migrations inside the API startup, create a dedicated migration project:

```
src/
├── Migrations/
│   └── HexMaster.BattleShip.Migrations/   ← runs EF Core migrations, then exits
├── HexMaster.BattleShip.Api/              ← waits for migration to complete
```

In the AppHost:

```csharp
var db = builder.AddPostgres("pg").AddDatabase("gamedb");

var migrator = builder.AddProject<Projects.Migrator>("migrator")
    .WithReference(db)
    .WaitFor(db);

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db)
    .WaitForCompletion(migrator);
```

Benefits:
- API never runs with an outdated schema
- Migration failures are visible in the Dashboard
- Migrations run exactly once, not on every API restart

---

## Persistent vs transient containers

### Transient (default)
Data is lost when the container stops:

```csharp
var redis = builder.AddRedis("cache");  // data lost on restart
```

### Persistent (with volumes)
Data survives container restarts:

```csharp
var postgres = builder.AddPostgres("pg")
    .WithDataVolume("battleship-pg-data");  // named Docker volume
```

### Volume naming strategies

| Method | Behavior |
|---|---|
| `.WithDataVolume()` | Auto-generated volume name |
| `.WithDataVolume("name")` | Explicit named volume |
| `.WithDataBindMount("./data")` | Bind mount to host filesystem |

---

## Environment-aware configuration

Use `builder.ExecutionContext.IsPublishMode` to vary configuration between development and deployment:

```csharp
var postgres = builder.AddPostgres("pg");

if (!builder.ExecutionContext.IsPublishMode)
{
    postgres.WithPgAdmin();  // only add pgAdmin in development
}
```

You can also use `builder.Configuration` to read environment-specific settings:

```csharp
var useEmulator = builder.Configuration.GetValue<bool>("UseStorageEmulator");
var storage = builder.AddAzureStorage("storage");
if (useEmulator)
    storage.RunAsEmulator();
```

---

## Hands-on exercise

1. **Add `WaitFor()` to the AppHost**
   - Ensure the API waits for any infrastructure resources to be healthy before starting
   - Run the app and observe the startup sequence in the Dashboard

2. **Create a migration project** (conceptual)
   - Sketch what a migration project would look like for the Battle Ops game database
   - Add `WaitForCompletion()` in the AppHost to enforce ordering

3. **Add a persistent volume**
   - Add `.WithDataVolume()` to the state store or a database resource
   - Restart the application and verify data persists

4. **Experiment with environment-aware config**
   - Add a conditional resource that only appears in development
   - Verify it doesn't appear in publish mode

---

## Key takeaways

- `WithReference()` wires connection information; `WaitFor()` and `WaitForCompletion()` control startup order
- The migrations-as-a-service pattern ensures schema consistency
- Named volumes persist data across container restarts
- Environment-aware configuration lets you tailor the topology for dev vs production
