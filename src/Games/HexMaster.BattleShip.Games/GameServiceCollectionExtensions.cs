using System.Reflection;
using HexMaster.BattleShip.Core.DependencyInjection;
using HexMaster.BattleShip.Games.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BattleShip.Games;

public static class GameServiceCollectionExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services)
    {
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IGameSecretHasher, Pbkdf2GameSecretHasher>();
        services.AddSingleton<IGameCodeGenerator, RandomGameCodeGenerator>();
        services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
