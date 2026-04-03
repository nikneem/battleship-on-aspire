using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HexMaster.BattleShip.Games;

internal static class GamesTelemetry
{
    public const string SourceName = "HexMaster.BattleShip.Games";

    public static readonly ActivitySource Source = new(SourceName);

    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<int> GamesCreated =
        Meter.CreateCounter<int>(
            "battleship.games.created",
            description: "Number of games created");

    public static readonly Counter<int> GamesJoined =
        Meter.CreateCounter<int>(
            "battleship.games.joined",
            description: "Number of players that joined a game");

    public static readonly Counter<int> ShotsFired =
        Meter.CreateCounter<int>(
            "battleship.games.shots.fired",
            description: "Number of shots fired");

    public static readonly Counter<int> GamesFinished =
        Meter.CreateCounter<int>(
            "battleship.games.finished",
            description: "Number of games finished");

    public static readonly Counter<int> GamesCancelled =
        Meter.CreateCounter<int>(
            "battleship.games.cancelled",
            description: "Number of games cancelled");

    public static readonly Counter<int> GamesAbandoned =
        Meter.CreateCounter<int>(
            "battleship.games.abandoned",
            description: "Number of games abandoned");
}
