using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HexMaster.BattleShip.Profiles;

internal static class ProfilesTelemetry
{
    public const string SourceName = "HexMaster.BattleShip.Profiles";

    public static readonly ActivitySource Source = new(SourceName);

    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<int> SessionsCreated =
        Meter.CreateCounter<int>(
            "battleship.profiles.sessions.created",
            description: "Number of anonymous player sessions created");

    public static readonly Counter<int> SessionsRenewed =
        Meter.CreateCounter<int>(
            "battleship.profiles.sessions.renewed",
            description: "Number of anonymous player sessions renewed");
}
