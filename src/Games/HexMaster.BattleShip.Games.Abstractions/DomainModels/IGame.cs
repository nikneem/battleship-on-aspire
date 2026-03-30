using HexMaster.BattleShip.Games.Abstractions.Models;

namespace HexMaster.BattleShip.Games.Abstractions.DomainModels;

public interface IPlayerBoard
{
    bool HasSubmittedFleet { get; }

    bool IsLocked { get; }

    IReadOnlyList<GameShipPlacement> Ships { get; }

    IReadOnlyList<GameShotRecord> IncomingShots { get; }
}

public interface IGamePlayerSlot
{
    string PlayerId { get; }

    string PlayerName { get; }

    bool IsReady { get; }

    IPlayerBoard Board { get; }
}

public interface IGame
{
    string GameCode { get; }

    GamePhase Phase { get; }

    bool IsProtected { get; }

    string? CurrentTurnPlayerId { get; }

    string? WinnerPlayerId { get; }

    bool HasChanges { get; }

    IGamePlayerSlot Host { get; }

    IGamePlayerSlot? Guest { get; }

    void JoinGuest(string playerId, string playerName, bool secretValidated);

    void MarkReady(string playerId);

    void SubmitFleet(string playerId, IReadOnlyCollection<GameShipPlacement> ships);

    void LockFleet(string playerId);

    ShotOutcome FireShot(string playerId, GameCoordinate target);

    void Cancel(string playerId);

    void Abandon(string playerId);
}
