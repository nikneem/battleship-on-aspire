using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.IntegrationEvents;
using HexMaster.BattleShip.Realtime.Abstractions.Connections;
using HexMaster.BattleShip.Realtime.Abstractions.Timers;
using Microsoft.AspNetCore.SignalR;

namespace HexMaster.BattleShip.Realtime.Hubs;

public sealed class GameHub(
    IGameConnectionTracker connectionTracker,
    IScheduledTimerService timerService,
    IEventBus eventBus,
    IHubContext<GameHub> hubContext) : Hub
{
    private static string TimerId(string gameCode, string playerId) => $"{gameCode}:{playerId}";

    public async Task JoinGame(string gameCode, string playerId)
    {
        var timerId = TimerId(gameCode, playerId);
        if (timerService.Cancel(timerId))
        {
            await eventBus.PublishAsync(
                new PlayerConnectionReestablishedIntegrationEvent(gameCode, playerId));
        }

        connectionTracker.TrackConnection(Context.ConnectionId, gameCode, playerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (connectionTracker.TryGetConnection(Context.ConnectionId, out var info))
        {
            if (exception != null)
            {
                await eventBus.PublishAsync(
                    new PlayerConnectionLostIntegrationEvent(info.GameCode, info.PlayerId, DateTimeOffset.UtcNow));

                await hubContext.Clients.Group(info.GameCode)
                    .SendAsync("OpponentConnectionLost", info.PlayerId);

                timerService.Schedule(
                    TimerId(info.GameCode, info.PlayerId),
                    TimeSpan.FromSeconds(60),
                    ct => eventBus.PublishAsync(
                        new PlayerConnectionTimedOutIntegrationEvent(info.GameCode, info.PlayerId), ct));
            }
            else
            {
                connectionTracker.RemoveConnection(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, info.GameCode);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
