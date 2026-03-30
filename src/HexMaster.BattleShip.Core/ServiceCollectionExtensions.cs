using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BattleShip.Core;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] HandlerInterfaceDefinitions =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    public static IServiceCollection AddHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        foreach (var implementationType in assembly.DefinedTypes
                     .Where(type => type is { IsAbstract: false, IsClass: true }))
        {
            var handlerInterfaces = implementationType.ImplementedInterfaces
                .Where(
                    serviceType => serviceType.IsGenericType &&
                                   HandlerInterfaceDefinitions.Contains(serviceType.GetGenericTypeDefinition()))
                .ToArray();

            foreach (var handlerInterface in handlerInterfaces)
            {
                if (services.Any(
                        descriptor => descriptor.ServiceType == handlerInterface &&
                                      descriptor.ImplementationType == implementationType.AsType()))
                {
                    continue;
                }

                services.Add(new ServiceDescriptor(handlerInterface, implementationType.AsType(), lifetime));
            }
        }

        return services;
    }
}
