# Module 05 — Extended Integration and Lifecycle Hooks

> **Level:** Intermediate–Advanced · **Duration:** 45–60 min

## Learning objectives

- Understand `IDistributedApplicationLifecycleHook` and eventing subscribers
- Know common use cases: data seeding, resource creation, health checks
- Work with annotations and how to use them
- Build a lifecycle hook that seeds initial data

---

## Lifecycle hooks overview

Aspire provides two extension points for running code at specific moments during the application lifecycle:

### `IDistributedApplicationLifecycleHook`

A simple interface with methods called at key lifecycle stages:

```csharp
public interface IDistributedApplicationLifecycleHook
{
    Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken ct);
    Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken ct);
    Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken ct);
}
```

| Method | When it runs |
|---|---|
| `BeforeStartAsync` | Before any resource has started |
| `AfterEndpointsAllocatedAsync` | After endpoints are assigned but before resources start |
| `AfterResourcesCreatedAsync` | After all resources are running and healthy |

### `IDistributedApplicationEventingSubscriber`

A more granular eventing model:

```csharp
builder.Eventing.Subscribe<BeforeResourceStartedEvent>(async (evt, ct) =>
{
    if (evt.Resource.Name == "battleship-api")
    {
        // Run setup code before the API starts
    }
});
```

---

## Common use cases

### Data seeding

Seed initial data after a database is running:

```csharp
public class SeedDataHook : IDistributedApplicationLifecycleHook
{
    public async Task AfterResourcesCreatedAsync(
        DistributedApplicationModel appModel, CancellationToken ct)
    {
        var db = appModel.Resources.OfType<PostgresDatabaseResource>()
            .FirstOrDefault(r => r.Name == "gamedb");

        if (db != null)
        {
            var connectionString = await db.ConnectionStringExpression
                .GetValueAsync(ct);
            // Execute seed SQL
        }
    }
}
```

### Creating cloud resources

Create Azure Storage queues or Blob containers before the application starts:

```csharp
public async Task AfterEndpointsAllocatedAsync(
    DistributedApplicationModel appModel, CancellationToken ct)
{
    // Create queues that the application expects to exist
    var queueClient = new QueueServiceClient(connectionString);
    await queueClient.CreateQueueAsync("game-events", cancellationToken: ct);
}
```

### Custom health checks

Register a health check that verifies external dependencies:

```csharp
builder.Eventing.Subscribe<BeforeResourceStartedEvent>(async (evt, ct) =>
{
    // Verify external service is reachable before starting
});
```

---

## Annotations

Annotations are metadata attached to resources. They're used by Aspire internally and can be used by your hooks:

```csharp
// Add a custom annotation
var api = builder.AddProject<Projects.Api>("api");
api.WithAnnotation(new CustomAnnotation("value"));

// Read annotations in a hook
public async Task BeforeStartAsync(
    DistributedApplicationModel appModel, CancellationToken ct)
{
    foreach (var resource in appModel.Resources)
    {
        var annotation = resource.Annotations
            .OfType<CustomAnnotation>()
            .FirstOrDefault();

        if (annotation != null)
        {
            // Act on the annotation
        }
    }
}
```

Built-in annotations include:
- `EndpointAnnotation` — HTTP/HTTPS endpoint information
- `EnvironmentCallbackAnnotation` — environment variable injection
- `ContainerImageAnnotation` — Docker image details

---

## Registering hooks

```csharp
// In AppHost Program.cs / AppHost.cs
builder.Services.AddLifecycleHook<SeedDataHook>();
```

Or with the eventing model:

```csharp
builder.Eventing.Subscribe<AfterResourcesCreatedEvent>(async (evt, ct) =>
{
    // Inline handler
});
```

---

## Hands-on exercise

1. **Create a data seeding hook**
   - Create a class that implements `IDistributedApplicationLifecycleHook`
   - In `AfterResourcesCreatedAsync`, log a message for each running resource
   - Register it in the AppHost
   - Run the app and verify your messages appear in the console

2. **Use the eventing model**
   - Subscribe to `BeforeResourceStartedEvent` for the API resource
   - Log the resource name and allocated endpoints
   - Verify the output

3. **Read annotations**
   - In your hook, enumerate resources and log their endpoint annotations
   - This shows you the full topology that Aspire has configured

4. **Real-world exercise: Auto-create Azure Storage queues**
   - If using Azure Storage with the emulator, create a hook that ensures required queues exist before the API starts
   - This simulates a production scenario where infrastructure must be provisioned

---

## Key takeaways

- Lifecycle hooks let you run code at specific stages of the Aspire application lifecycle
- Use `AfterResourcesCreatedAsync` for data seeding (resources are healthy at this point)
- The eventing model (`Subscribe<TEvent>`) gives fine-grained control per resource
- Annotations carry metadata that hooks and extensions can read and act on
- Hooks are the right place for one-time setup, not ongoing background work
