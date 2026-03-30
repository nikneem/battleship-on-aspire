using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.DomainModels;
using HexMaster.BattleShip.Games.Abstractions.Models;

namespace HexMaster.BattleShip.Games;

internal static class GameMappings
{
    public static CreateGameResponseDto ToCreateGameResponseDto(IGame game)
    {
        return new CreateGameResponseDto(
            game.GameCode,
            game.Phase,
            game.IsProtected ? GameLobbyProtectionVisibility.Protected : GameLobbyProtectionVisibility.Open,
            IsJoinable(game),
            ToParticipantDto(game, game.Host));
    }

    public static GameLobbyResponseDto ToLobbyResponseDto(IGame game)
    {
        return new GameLobbyResponseDto(
            game.GameCode,
            game.Phase,
            game.IsProtected ? GameLobbyProtectionVisibility.Protected : GameLobbyProtectionVisibility.Open,
            IsJoinable(game),
            ToParticipantDto(game, game.Host),
            game.Guest is null ? null : ToParticipantDto(game, game.Guest));
    }

    public static GameStateResponseDto ToStateResponseDto(IGame game, string playerId)
    {
        var currentPlayer = ResolvePlayer(game, playerId);
        var opponent = ResolveOpponent(game, playerId);

        return new GameStateResponseDto(
            game.GameCode,
            game.Phase,
            game.CurrentTurnPlayerId,
            game.WinnerPlayerId,
            ToParticipantDto(game, currentPlayer),
            opponent is null ? null : ToParticipantDto(game, opponent),
            new PlayerBoardStateDto(
                currentPlayer.Board.IsLocked,
                currentPlayer.Board.Ships.Select(ToShipPlacementDto).ToArray(),
                currentPlayer.Board.IncomingShots.Select(ToShotRecordDto).ToArray()),
            new OpponentBoardStateDto(opponent?.Board.IncomingShots.Select(ToShotRecordDto).ToArray() ?? []));
    }

    public static IReadOnlyList<GameShipPlacement> ToShipPlacements(IReadOnlyList<GameShipPlacementDto> ships)
    {
        ArgumentNullException.ThrowIfNull(ships);
        return ships.Select(static ship => new GameShipPlacement(ship.Length, ToCoordinate(ship.Start), ship.Orientation)).ToArray();
    }

    public static GameCoordinate ToCoordinate(GameCoordinateDto coordinate)
    {
        ArgumentNullException.ThrowIfNull(coordinate);
        return new GameCoordinate(coordinate.Row, coordinate.Column);
    }

    private static GameParticipantDto ToParticipantDto(IGame game, IGamePlayerSlot slot)
    {
        return new GameParticipantDto(slot.PlayerId, slot.PlayerName, ToProjection(game, slot));
    }

    private static PlayerGameStateProjection ToProjection(IGame game, IGamePlayerSlot slot)
    {
        return game.Phase switch
        {
            GamePhase.LobbyOpen => PlayerGameStateProjection.WaitingForOpponent,
            GamePhase.LobbyFull => PlayerGameStateProjection.AwaitingReady,
            GamePhase.Setup => slot.Board.IsLocked
                ? PlayerGameStateProjection.FleetLocked
                : PlayerGameStateProjection.AwaitingFleet,
            GamePhase.InProgress => string.Equals(game.CurrentTurnPlayerId, slot.PlayerId, StringComparison.Ordinal)
                ? PlayerGameStateProjection.YourTurn
                : PlayerGameStateProjection.OpponentTurn,
            GamePhase.Finished => string.Equals(game.WinnerPlayerId, slot.PlayerId, StringComparison.Ordinal)
                ? PlayerGameStateProjection.Winner
                : PlayerGameStateProjection.Loser,
            GamePhase.Cancelled => PlayerGameStateProjection.Cancelled,
            GamePhase.Abandoned => PlayerGameStateProjection.Abandoned,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static GameShipPlacementDto ToShipPlacementDto(GameShipPlacement placement)
    {
        return new GameShipPlacementDto(placement.Length, ToCoordinateDto(placement.Start), placement.Orientation);
    }

    private static ShotRecordDto ToShotRecordDto(GameShotRecord shot)
    {
        return new ShotRecordDto(ToCoordinateDto(shot.Coordinate), shot.Outcome);
    }

    private static GameCoordinateDto ToCoordinateDto(GameCoordinate coordinate) => new(coordinate.Row, coordinate.Column);

    private static bool IsJoinable(IGame game) => game.Phase == GamePhase.LobbyOpen && game.Guest is null;

    private static IGamePlayerSlot ResolvePlayer(IGame game, string playerId)
    {
        if (string.Equals(game.Host.PlayerId, playerId, StringComparison.Ordinal))
        {
            return game.Host;
        }

        if (game.Guest is not null && string.Equals(game.Guest.PlayerId, playerId, StringComparison.Ordinal))
        {
            return game.Guest;
        }

        throw new UnauthorizedAccessException("Only joined players can access game state.");
    }

    private static IGamePlayerSlot? ResolveOpponent(IGame game, string playerId)
    {
        if (string.Equals(game.Host.PlayerId, playerId, StringComparison.Ordinal))
        {
            return game.Guest;
        }

        if (game.Guest is not null && string.Equals(game.Guest.PlayerId, playerId, StringComparison.Ordinal))
        {
            return game.Host;
        }

        throw new UnauthorizedAccessException("Only joined players can access game state.");
    }
}
