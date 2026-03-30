namespace HexMaster.BattleShip.Games.Abstractions.Models;

public enum GamePhase
{
    LobbyOpen = 0,
    LobbyFull = 1,
    Setup = 2,
    InProgress = 3,
    Finished = 4,
    Cancelled = 5,
    Abandoned = 6
}

public enum GameLobbyProtectionVisibility
{
    Open = 0,
    Protected = 1
}

public enum ShotOutcome
{
    Miss = 0,
    Hit = 1,
    Sunk = 2
}

public enum PlayerGameStateProjection
{
    WaitingForOpponent = 0,
    AwaitingReady = 1,
    AwaitingFleet = 2,
    FleetLocked = 3,
    YourTurn = 4,
    OpponentTurn = 5,
    Winner = 6,
    Loser = 7,
    Cancelled = 8,
    Abandoned = 9
}

public enum ShipOrientation
{
    Horizontal = 0,
    Vertical = 1
}

public readonly record struct GameCoordinate(int Row, int Column);

public sealed record GameShipPlacement(int Length, GameCoordinate Start, ShipOrientation Orientation);

public sealed record GameShotRecord(GameCoordinate Coordinate, ShotOutcome Outcome);
