# Module 07 — Automated Testing

> **Level:** Intermediate–Advanced · **Duration:** 60–75 min

## Learning objectives

- Understand the three-tier testing strategy: Unit → Integration → E2E
- Use `DistributedApplicationTestingBuilder` and `DistributedApplicationFactory`
- Customize the AppHost for tests using the subclassing pattern
- Achieve test isolation with volume suffixes and database reset (Respawn)
- Wait for resources with `WaitForResourceHealthyAsync`
- Apply environment-specific configuration in tests

---

## Three-tier testing strategy

| Tier | Scope | Speed | Infrastructure |
|---|---|---|---|
| **Unit** | Single class/method | Fast (ms) | None — pure mocking |
| **Integration** | Multiple services + real infra | Medium (seconds) | Aspire manages containers |
| **E2E** | Full user flow | Slow (minutes) | Full application stack |

Battle Ops already has unit tests in each domain's `.Tests` project. This module focuses on **integration tests** that use real infrastructure managed by Aspire.

---

## DistributedApplicationTestingBuilder

The testing builder creates a test instance of your AppHost:

```csharp
[Fact]
public async Task ApiReturnsHealthy()
{
    var builder = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.HexMaster_BattleShip_Aspire_AppHost>();

    await using var app = await builder.BuildAsync();
    await app.StartAsync();

    // Wait for the API to be healthy
    await app.WaitForResourceHealthyAsync("battleship-api");

    // Get the HTTP client for the API
    var httpClient = app.CreateHttpClient("battleship-api");
    var response = await httpClient.GetAsync("/health");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

---

## DistributedApplicationFactory (subclassing pattern)

For more control, subclass `DistributedApplicationFactory`:

```csharp
public class BattleShipAppFactory
    : DistributedApplicationFactory<Projects.HexMaster_BattleShip_Aspire_AppHost>
{
    protected override void OnBuilderCreating(
        DistributedApplicationOptions options,
        IConfiguration configuration)
    {
        // Override configuration for tests
        options.Args = ["--environment", "Testing"];
    }

    protected override void OnBuilderCreated(
        DistributedApplicationBuilder builder)
    {
        // Customize resources for testing
        // e.g., use different volume names to isolate test data
    }
}
```

Usage:

```csharp
[Fact]
public async Task CreateGameReturnsGameCode()
{
    await using var app = new BattleShipAppFactory();
    await app.StartAsync();
    await app.WaitForResourceHealthyAsync("battleship-api");

    var client = app.CreateHttpClient("battleship-api");

    // Create an anonymous session first
    var sessionResponse = await client.PostAsync(
        "/api/profiles/anonymous-sessions", null);
    var session = await sessionResponse.Content
        .ReadFromJsonAsync<SessionDto>();

    // Use the token to create a game
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", session!.Token);

    var createResponse = await client.PostAsJsonAsync(
        "/api/games", new { });

    Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
}
```

---

## Test isolation

### Volume suffixes

Prevent test data from polluting development data:

```csharp
protected override void OnBuilderCreated(
    DistributedApplicationBuilder builder)
{
    // Each test run gets a unique volume suffix
    var suffix = Guid.NewGuid().ToString("N")[..8];

    // Override volume names for isolation
    foreach (var resource in builder.Resources.OfType<ContainerResource>())
    {
        // Modify volume annotations to use suffixed names
    }
}
```

### Database reset with Respawn

Reset the database between tests without recreating the container:

```csharp
private async Task ResetDatabaseAsync(string connectionString)
{
    var respawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions
    {
        DbAdapter = DbAdapter.Postgres,
        TablesToIgnore = ["__EFMigrationsHistory"]
    });

    await respawner.ResetAsync(connectionString);
}
```

---

## WaitForResourceHealthyAsync

Always wait for resources before making HTTP calls:

```csharp
// Wait for a specific resource
await app.WaitForResourceHealthyAsync("battleship-api");

// Wait with a timeout
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
await app.WaitForResourceHealthyAsync("battleship-api", cts.Token);
```

This prevents flaky tests caused by resources still starting up.

---

## Environment-specific configuration

Tests can use a different environment:

```csharp
// In the test factory
protected override void OnBuilderCreating(
    DistributedApplicationOptions options,
    IConfiguration configuration)
{
    options.Args = ["--environment", "Testing"];
}
```

In `appsettings.Testing.json`:
```json
{
  "UseInMemoryDatabase": true,
  "Jwt": {
    "SigningKey": "test-key-for-integration-tests-only"
  }
}
```

---

## Hands-on exercise

1. **Create an integration test project**
   - Add a new xUnit project: `HexMaster.BattleShip.IntegrationTests`
   - Reference the AppHost project
   - Install `Aspire.Hosting.Testing`

2. **Write a health check test**
   - Use `DistributedApplicationTestingBuilder` to start the app
   - Wait for the API to be healthy
   - Assert the `/health` endpoint returns 200

3. **Write a game creation test**
   - Create an anonymous session
   - Use the JWT to create a game
   - Assert the response includes a game code

4. **Add database reset**
   - If PostgreSQL is configured, use Respawn to reset between tests
   - Verify that test data from one test doesn't leak into another

---

## Key takeaways

- Aspire's testing infrastructure lets you spin up the full distributed application in tests
- `DistributedApplicationTestingBuilder` is the quick-start path; subclassing gives more control
- Always use `WaitForResourceHealthyAsync` before making requests
- Isolate test data with volume suffixes or Respawn
- Integration tests complement unit tests by testing real service interactions
