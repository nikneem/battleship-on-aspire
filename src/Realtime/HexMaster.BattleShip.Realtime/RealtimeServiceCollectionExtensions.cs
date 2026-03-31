using System.Reflection;
using HexMaster.BattleShip.Core.DependencyInjection;
using HexMaster.BattleShip.Realtime.Abstractions.Connections;
using HexMaster.BattleShip.Realtime.Abstractions.Timers;
using HexMaster.BattleShip.Realtime.Connections;
using HexMaster.BattleShip.Realtime.Hubs;
using HexMaster.BattleShip.Realtime.Timers;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BattleShip.Realtime;

public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeModule(this IServiceCollection services)
    {
        services.AddSingleton<IScheduledTimerService, ScheduledTimerService>();
        services.AddSingleton<IGameConnectionTracker, InMemoryGameConnectionTracker>();

        services.AddSignalR();

        services.AddHandlersFromAssembly(typeof(RealtimeServiceCollectionExtensions).Assembly);

        return services;
    }
}
