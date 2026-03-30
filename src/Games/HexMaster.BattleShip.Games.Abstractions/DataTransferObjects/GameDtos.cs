using HexMaster.BattleShip.Games.Abstractions.Models;

namespace HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;

public sealed record CreateGameRequestDto(string? JoinSecret);

public sealed record CreateGameResponseDto(
    string GameCode,
    GamePhase Phase,
    GameLobbyProtectionVisibility Protection,
    bool IsJoinable,
    GameParticipantDto Host);

public sealed record JoinGameByCodeRequestDto(string GameCode, string? JoinSecret);

public sealed record MarkReadyRequestDto();

public sealed record SubmitFleetRequestDto(IReadOnlyList<GameShipPlacementDto> Ships);

public sealed record LockFleetRequestDto();

public sealed record FireShotRequestDto(GameCoordinateDto Target);

public sealed record CancelGameRequestDto();

public sealed record AbandonGameRequestDto();

public sealed record GameLobbyResponseDto(
    string GameCode,
    GamePhase Phase,
    GameLobbyProtectionVisibility Protection,
    bool IsJoinable,
    GameParticipantDto Host,
    GameParticipantDto? Guest);

public sealed record GameStateResponseDto(
    string GameCode,
    GamePhase Phase,
    string? CurrentTurnPlayerId,
    string? WinnerPlayerId,
    GameParticipantDto CurrentPlayer,
    GameParticipantDto? Opponent,
    PlayerBoardStateDto OwnBoard,
    OpponentBoardStateDto OpponentBoard);

public sealed record GameParticipantDto(
    string PlayerId,
    string PlayerName,
    PlayerGameStateProjection State);

public sealed record GameCoordinateDto(int Row, int Column);

public sealed record GameShipPlacementDto(
    int Length,
    GameCoordinateDto Start,
    ShipOrientation Orientation);

public sealed record ShotRecordDto(GameCoordinateDto Coordinate, ShotOutcome Outcome);

public sealed record PlayerBoardStateDto(
    bool IsLocked,
    IReadOnlyList<GameShipPlacementDto> Ships,
    IReadOnlyList<ShotRecordDto> IncomingShots);

public sealed record OpponentBoardStateDto(IReadOnlyList<ShotRecordDto> KnownShots);
