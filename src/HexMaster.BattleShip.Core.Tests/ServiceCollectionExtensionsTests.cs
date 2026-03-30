using System.Reflection;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BattleShip.Core.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHandlersFromAssembly_RegistersCommandAndQueryHandlers()
    {
        var services = new ServiceCollection();

        services.AddHandlersFromAssembly(typeof(ServiceCollectionExtensionsTests).Assembly);

        var commandDescriptor = Assert.Single(
            services.Where(descriptor => descriptor.ServiceType == typeof(ICommandHandler<TestCommand, string>)));
        var queryDescriptor = Assert.Single(
            services.Where(descriptor => descriptor.ServiceType == typeof(IQueryHandler<TestQuery, string>)));

        Assert.Equal(typeof(TestCommandHandler), commandDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, commandDescriptor.Lifetime);
        Assert.Equal(typeof(TestQueryHandler), queryDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, queryDescriptor.Lifetime);
    }

    [Fact]
    public void AddHandlersFromAssembly_DoesNotAddDuplicateRegistrations()
    {
        var services = new ServiceCollection();
        var assembly = typeof(ServiceCollectionExtensionsTests).Assembly;

        services.AddHandlersFromAssembly(assembly);
        services.AddHandlersFromAssembly(assembly);

        Assert.Single(services.Where(
            descriptor => descriptor.ServiceType == typeof(ICommandHandler<TestCommand, string>)));
        Assert.Single(services.Where(
            descriptor => descriptor.ServiceType == typeof(IQueryHandler<TestQuery, string>)));
    }

    [Fact]
    public void AddHandlersFromAssembly_UsesRequestedLifetime()
    {
        var services = new ServiceCollection();

        services.AddHandlersFromAssembly(typeof(ServiceCollectionExtensionsTests).Assembly, ServiceLifetime.Singleton);

        Assert.All(
            services.Where(descriptor =>
                descriptor.ServiceType == typeof(ICommandHandler<TestCommand, string>) ||
                descriptor.ServiceType == typeof(IQueryHandler<TestQuery, string>)),
            descriptor => Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime));
    }

    public sealed record TestCommand() : ICommand<string>;

    public sealed class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult("ok");
    }

    public sealed record TestQuery() : IQuery<string>;

    public sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult("ok");
    }
}
