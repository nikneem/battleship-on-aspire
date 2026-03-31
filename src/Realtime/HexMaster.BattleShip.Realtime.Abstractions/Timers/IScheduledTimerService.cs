namespace HexMaster.BattleShip.Realtime.Abstractions.Timers;

public interface IScheduledTimerService
{
    /// <summary>
    /// Schedules a one-shot callback after <paramref name="delay"/>.
    /// Silently ignored if an entry with the same <paramref name="id"/> already exists.
    /// </summary>
    void Schedule(string id, TimeSpan delay, Func<CancellationToken, Task> callback);

    /// <summary>
    /// Cancels the pending timer with <paramref name="id"/>.
    /// Returns <c>false</c> if no timer with that id was found.
    /// </summary>
    bool Cancel(string id);
}
