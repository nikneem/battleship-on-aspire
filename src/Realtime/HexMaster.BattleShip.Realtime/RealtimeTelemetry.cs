using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HexMaster.BattleShip.Realtime;

internal static class RealtimeTelemetry
{
    public const string SourceName = "HexMaster.BattleShip.Realtime";

    public static readonly ActivitySource Source = new(SourceName);

    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<int> PlayerConnectionsJoined =
        Meter.CreateCounter<int>(
            "battleship.realtime.connections.joined",
            description: "Number of players that joined a game via SignalR");

    public static readonly Counter<int> PlayerConnectionsLost =
        Meter.CreateCounter<int>(
            "battleship.realtime.connections.lost",
            description: "Number of player connections lost unexpectedly");
}
