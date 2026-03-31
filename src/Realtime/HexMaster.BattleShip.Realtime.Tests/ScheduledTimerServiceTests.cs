using HexMaster.BattleShip.Realtime.Timers;
using Microsoft.Extensions.Logging.Abstractions;

namespace HexMaster.BattleShip.Realtime.Tests;

public sealed class ScheduledTimerServiceTests
{
    [Fact]
    public async Task CallbackFiresAfterDelay()
    {
        var sut = new ScheduledTimerService(NullLogger<ScheduledTimerService>.Instance);
        var fired = false;

        sut.Schedule("t1", TimeSpan.FromMilliseconds(50), _ =>
        {
            fired = true;
            return Task.CompletedTask;
        });

        await Task.Delay(200);
        Assert.True(fired);
    }

    [Fact]
    public async Task CallbackDoesNotFireAfterCancel()
    {
        var sut = new ScheduledTimerService(NullLogger<ScheduledTimerService>.Instance);
        var fired = false;

        sut.Schedule("t2", TimeSpan.FromMilliseconds(100), _ =>
        {
            fired = true;
            return Task.CompletedTask;
        });

        var cancelled = sut.Cancel("t2");

        await Task.Delay(250);
        Assert.True(cancelled);
        Assert.False(fired);
    }

    [Fact]
    public void CancelReturnsFalseForUnknownId()
    {
        var sut = new ScheduledTimerService(NullLogger<ScheduledTimerService>.Instance);
        Assert.False(sut.Cancel("unknown"));
    }

    [Fact]
    public async Task DuplicateScheduleForSameIdIsIgnored()
    {
        var sut = new ScheduledTimerService(NullLogger<ScheduledTimerService>.Instance);
        var callCount = 0;

        sut.Schedule("t3", TimeSpan.FromMilliseconds(50), _ =>
        {
            callCount++;
            return Task.CompletedTask;
        });

        sut.Schedule("t3", TimeSpan.FromMilliseconds(50), _ =>
        {
            callCount++;
            return Task.CompletedTask;
        });

        await Task.Delay(200);
        Assert.Equal(1, callCount);
    }
}
