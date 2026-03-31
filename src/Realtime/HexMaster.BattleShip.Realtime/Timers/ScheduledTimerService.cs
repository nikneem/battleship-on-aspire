using System.Collections.Concurrent;
using HexMaster.BattleShip.Realtime.Abstractions.Timers;
using Microsoft.Extensions.Logging;

namespace HexMaster.BattleShip.Realtime.Timers;

public sealed class ScheduledTimerService(ILogger<ScheduledTimerService> logger) : IScheduledTimerService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _timers = new();

    public void Schedule(string id, TimeSpan delay, Func<CancellationToken, Task> callback)
    {
        var cts = new CancellationTokenSource();
        if (!_timers.TryAdd(id, cts))
        {
            cts.Dispose();
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cts.Token);
                _timers.TryRemove(id, out _);
                await callback(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Timer was cancelled — expected path
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduled timer {TimerId} callback threw an unhandled exception", id);
            }
            finally
            {
                cts.Dispose();
            }
        }, cts.Token);
    }

    public bool Cancel(string id)
    {
        if (!_timers.TryRemove(id, out var cts))
        {
            return false;
        }

        cts.Cancel();
        cts.Dispose();
        return true;
    }
}
