using HexMaster.BattleShip.Games.Abstractions.Models;

namespace HexMaster.BattleShip.Games.Persistence;

internal sealed record GameDocument(
    string GameCode,
    GamePhase Phase,
    string? ProtectedSecretHash,
    string? CurrentTurnPlayerId,
    string? WinnerPlayerId,
    PlayerSlotDocument Host,
    PlayerSlotDocument? Guest);

internal sealed record PlayerSlotDocument(
    string PlayerId,
    string PlayerName,
    bool IsReady,
    BoardDocument Board);

internal sealed record BoardDocument(
    bool IsLocked,
    IReadOnlyList<GameShipPlacement> Ships,
    IReadOnlyList<GameShotRecord> IncomingShots);
