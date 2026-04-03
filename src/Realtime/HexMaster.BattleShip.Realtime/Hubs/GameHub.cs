using System.Diagnostics;
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
        using var activity = RealtimeTelemetry.Source.StartActivity("JoinGame");
        activity?.SetTag("game.code", gameCode);
        activity?.SetTag("game.player_id", playerId);

        var timerId = TimerId(gameCode, playerId);
        if (timerService.Cancel(timerId))
        {
            await eventBus.PublishAsync(
                new PlayerConnectionReestablishedIntegrationEvent(gameCode, playerId));
        }

        connectionTracker.TrackConnection(Context.ConnectionId, gameCode, playerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);

        activity?.SetStatus(ActivityStatusCode.Ok);
        RealtimeTelemetry.PlayerConnectionsJoined.Add(1);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (connectionTracker.TryGetConnection(Context.ConnectionId, out var info))
        {
            if (exception != null)
            {
                using var activity = RealtimeTelemetry.Source.StartActivity("PlayerDisconnected");
                activity?.SetTag("game.code", info.GameCode);
                activity?.SetTag("game.player_id", info.PlayerId);

                await eventBus.PublishAsync(
                    new PlayerConnectionLostIntegrationEvent(info.GameCode, info.PlayerId, DateTimeOffset.UtcNow));

                await hubContext.Clients.Group(info.GameCode)
                    .SendAsync("OpponentConnectionLost", info.PlayerId);

                timerService.Schedule(
                    TimerId(info.GameCode, info.PlayerId),
                    TimeSpan.FromSeconds(60),
                    ct => eventBus.PublishAsync(
                        new PlayerConnectionTimedOutIntegrationEvent(info.GameCode, info.PlayerId), ct));

                RealtimeTelemetry.PlayerConnectionsLost.Add(1);
                activity?.SetStatus(ActivityStatusCode.Ok);
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
