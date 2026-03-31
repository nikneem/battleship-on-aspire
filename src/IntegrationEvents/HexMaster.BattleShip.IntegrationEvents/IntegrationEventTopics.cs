namespace HexMaster.BattleShip.IntegrationEvents;

public static class IntegrationEventTopics
{
    // Games domain topics
    public const string GameCreated = "battleship.game.game-created";
    public const string PlayerJoined = "battleship.game.player-joined";
    public const string PlayerMarkedReady = "battleship.game.player-marked-ready";
    public const string FleetSubmitted = "battleship.game.fleet-submitted";
    public const string FleetLocked = "battleship.game.fleet-locked";
    public const string GameStarted = "battleship.game.game-started";
    public const string ShotFired = "battleship.game.shot-fired";
    public const string GameFinished = "battleship.game.game-finished";
    public const string GameCancelled = "battleship.game.game-cancelled";
    public const string GameAbandoned = "battleship.game.game-abandoned";

    // Realtime / player connection topics
    public const string PlayerConnectionLost = "battleship.player.connection-lost";
    public const string PlayerConnectionReestablished = "battleship.player.connection-reestablished";
    public const string PlayerConnectionTimedOut = "battleship.player.connection-timed-out";
}
