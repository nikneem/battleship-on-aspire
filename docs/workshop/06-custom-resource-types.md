# Module 06 — Custom Resource Types

> **Level:** Advanced · **Duration:** 45–60 min

## Learning objectives

- Know when to build a custom resource vs use an existing integration
- Understand resource abstractions: `IResource`, `IResourceWithConnectionString`, etc.
- Build `IResourceBuilder` extension methods
- Work with annotations and customization
- Understand publishing to NuGet (brief overview)

---

## When to build your own

Build a custom resource when:
- ✅ No existing Aspire integration covers your technology
- ✅ You need to encapsulate complex container configuration
- ✅ You want a reusable abstraction shared across multiple AppHosts
- ✅ You need custom health checks or connection string formats

Use an existing integration when:
- ❌ An official `Aspire.Hosting.*` package exists
- ❌ A community package on NuGet covers your use case

---

## Resource abstractions

Aspire's resource model is built on interfaces:

```
IResource
├── IResourceWithConnectionString     (provides a connection string)
├── IResourceWithEnvironment          (supports environment variables)
├── IResourceWithEndpoints            (exposes HTTP/TCP endpoints)
└── ContainerResource                 (base class for container resources)
    └── Your custom container
```

### Minimal custom resource

```csharp
public class MyServiceResource(string name)
    : ContainerResource(name), IResourceWithConnectionString
{
    // Internal port the container listens on
    internal const int DefaultPort = 8080;

    // Connection string expression used by referencing services
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{Name}:{DefaultPort}");
}
```

---

## IResourceBuilder extension methods

Extension methods provide the fluent API that AppHost authors use:

```csharp
public static class MyServiceResourceBuilderExtensions
{
    public static IResourceBuilder<MyServiceResource> AddMyService(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        var resource = new MyServiceResource(name);

        return builder.AddResource(resource)
            .WithImage("myregistry/myservice")
            .WithImageTag("latest")
            .WithHttpEndpoint(
                port: port,
                targetPort: MyServiceResource.DefaultPort,
                name: "http");
    }

    public static IResourceBuilder<MyServiceResource> WithAdminUI(
        this IResourceBuilder<MyServiceResource> builder)
    {
        // Add a companion admin container
        return builder.WithAnnotation(
            new ContainerImageAnnotation { Image = "myregistry/admin-ui" });
    }
}
```

Usage in AppHost:

```csharp
var myService = builder.AddMyService("my-service", port: 9090)
    .WithAdminUI();

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(myService);
```

---

## Adding health checks

Custom resources should include health checks so `WaitFor()` works correctly:

```csharp
public static IResourceBuilder<MyServiceResource> AddMyService(
    this IDistributedApplicationBuilder builder, string name)
{
    var resource = new MyServiceResource(name);

    builder.Services.AddHealthChecks()
        .AddUrlGroup(
            new Uri($"http://localhost:{MyServiceResource.DefaultPort}/health"),
            name: $"{name}-health");

    return builder.AddResource(resource)
        .WithImage("myregistry/myservice")
        .WithHttpEndpoint(targetPort: MyServiceResource.DefaultPort)
        .WithHealthCheck($"{name}-health");
}
```

---

## Annotations and customization

Annotations attach metadata to resources. Use them to carry configuration:

```csharp
public class MyServiceConfigAnnotation(string configValue) : IResourceAnnotation
{
    public string ConfigValue { get; } = configValue;
}

// Extension method
public static IResourceBuilder<MyServiceResource> WithConfig(
    this IResourceBuilder<MyServiceResource> builder, string value)
{
    return builder.WithAnnotation(new MyServiceConfigAnnotation(value));
}
```

Lifecycle hooks and deployment manifests can read these annotations.

---

## Publishing to NuGet

When your custom resource is stable, you can package it as a NuGet package:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <PackageId>MyOrg.Aspire.Hosting.MyService</PackageId>
    <Description>Aspire hosting integration for MyService</Description>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting" />
  </ItemGroup>
</Project>
```

Follow the naming convention: `{Org}.Aspire.Hosting.{Technology}`

---

## Hands-on exercise

1. **Build a custom container resource**
   - Create a new class that extends `ContainerResource`
   - Implement `IResourceWithConnectionString`
   - Add an `AddX()` extension method

2. **Add it to the AppHost**
   - Use your extension method to add the resource
   - Reference it from the API project
   - Run the app and verify the container appears in the Dashboard

3. **Add a health check**
   - Register a health check for your custom resource
   - Use `WaitFor()` to ensure the API waits for it

4. **Add customization**
   - Create a `WithX()` extension method that adds an annotation
   - Read that annotation in a lifecycle hook

---

## Key takeaways

- Custom resources extend `ContainerResource` or implement `IResource` directly
- `IResourceWithConnectionString` enables `WithReference()` to inject connection strings
- Extension methods provide the fluent API (`AddX()`, `WithY()`)
- Health checks enable `WaitFor()` to work with your custom resources
- Package as NuGet for reuse across projects and teams
