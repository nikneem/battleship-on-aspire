using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.Features.AbandonGame;
using HexMaster.BattleShip.Games.Features.CreateGame;
using HexMaster.BattleShip.Games.Features.FireShot;
using HexMaster.BattleShip.Games.Features.GetGameStateForPlayer;
using HexMaster.BattleShip.Games.Features.JoinGameByCode;
using HexMaster.BattleShip.Games.Features.LockFleet;
using HexMaster.BattleShip.Games.Features.MarkReady;
using HexMaster.BattleShip.Games.Features.SubmitFleet;

namespace HexMaster.BattleShip.Games.Tests;

public sealed class GameCommandHandlerTests
{
    [Fact]
    public async Task CreateGameStoresProtectedLobbyAndHashesJoinSecret()
    {
        var repository = new InMemoryGameRepository();
        var secretHasher = new Pbkdf2GameSecretHasher();
        var createHandler = new CreateGameHandler(
            repository,
            new StubGameCodeGenerator("12345678"),
            secretHasher,
            new NullEventBus());

        var response = await createHandler.HandleAsync(new CreateGameCommand("host-1", "Admiral", "let-me-in"));
        var storedGame = await repository.GetByCodeAsync(response.GameCode);

        Assert.NotNull(storedGame);
        Assert.Equal("12345678", response.GameCode);
        Assert.Equal(GameLobbyProtectionVisibility.Protected, response.Protection);
        Assert.True(response.IsJoinable);
        Assert.Equal("Admiral", response.Host.PlayerName);
        Assert.True(storedGame.IsProtected);
    }

    [Fact]
    public async Task JoinProtectedGameRejectsWrongSecretAndAcceptsMatchingSecret()
    {
        var repository = new InMemoryGameRepository();
        var secretHasher = new Pbkdf2GameSecretHasher();
        var createHandler = new CreateGameHandler(
            repository,
            new StubGameCodeGenerator("12345678"),
            secretHasher,
            new NullEventBus());
        var joinHandler = new JoinGameByCodeHandler(repository, secretHasher, new NullEventBus());

        await createHandler.HandleAsync(new CreateGameCommand("host-1", "Admiral", "let-me-in"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            joinHandler.HandleAsync(
                new JoinGameByCodeCommand("12345678", "guest-1", "Commander", "wrong-secret")));

        var lobby = await joinHandler.HandleAsync(
            new JoinGameByCodeCommand("12345678", "guest-1", "Commander", "let-me-in"));

        Assert.Equal(GamePhase.LobbyFull, lobby.Phase);
        Assert.NotNull(lobby.Guest);
        Assert.Equal("Commander", lobby.Guest.PlayerName);
    }

    [Fact]
    public async Task FireShotAlternatesTurnsAfterEveryShot()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());

        var hostView = await fireShotHandler.HandleAsync(
            new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9)));

        Assert.Equal(GamePhase.InProgress, hostView.Phase);
        Assert.Equal("guest-1", hostView.CurrentTurnPlayerId);

        var guestView = await fireShotHandler.HandleAsync(
            new FireShotCommand("12345678", "guest-1", new GameCoordinate(9, 8)));

        Assert.Equal("host-1", guestView.CurrentTurnPlayerId);
    }

    [Fact]
    public async Task FireShotRejectsDuplicateCoordinateForOpponentBoard()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());

        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9)));
        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "guest-1", new GameCoordinate(9, 8)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9))));
    }

    [Fact]
    public async Task AbandonGameEndsJoinedGameWithoutWinner()
    {
        var repository = new InMemoryGameRepository();
        await CreateJoinedLobbyAsync(repository);
        var abandonHandler = new AbandonGameHandler(repository, new NullEventBus());

        var response = await abandonHandler.HandleAsync(new AbandonGameCommand("12345678", "guest-1"));

        Assert.Equal(GamePhase.Abandoned, response.Phase);
        Assert.Null(response.WinnerPlayerId);
        Assert.Null(response.CurrentTurnPlayerId);
    }

    [Fact]
    public async Task GetGameStateForPlayerReturnsOwnBoardAndOnlyKnownOpponentShots()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());
        var queryHandler = new GetGameStateForPlayerHandler(repository);

        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9)));

        var state = await queryHandler.HandleAsync(new GetGameStateForPlayerQuery("12345678", "host-1"));

        Assert.Equal(5, state.OwnBoard.Ships.Count);
        Assert.Single(state.OpponentBoard.KnownShots);
        Assert.Equal(new GameCoordinateDto(9, 9), state.OpponentBoard.KnownShots[0].Coordinate);
        Assert.Equal(PlayerGameStateProjection.OpponentTurn, state.CurrentPlayer.State);
    }

    private static async Task CreateJoinedLobbyAsync(InMemoryGameRepository repository)
    {
        var secretHasher = new Pbkdf2GameSecretHasher();
        var nullBus = new NullEventBus();
        var createHandler = new CreateGameHandler(
            repository,
            new StubGameCodeGenerator("12345678"),
            secretHasher,
            nullBus);
        var joinHandler = new JoinGameByCodeHandler(repository, secretHasher, nullBus);

        await createHandler.HandleAsync(new CreateGameCommand("host-1", "Admiral", null));
        await joinHandler.HandleAsync(new JoinGameByCodeCommand("12345678", "guest-1", "Commander", null));
    }

    private static async Task CreateReadyToPlayGameAsync(InMemoryGameRepository repository)
    {
        await CreateJoinedLobbyAsync(repository);

        var nullBus = new NullEventBus();
        var markReadyHandler = new MarkReadyHandler(repository, nullBus);
        var submitFleetHandler = new SubmitFleetHandler(repository, nullBus);
        var lockFleetHandler = new LockFleetHandler(repository, nullBus);

        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "host-1"));
        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "guest-1"));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "host-1", CreateHostFleet()));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "guest-1", CreateGuestFleet()));
        await lockFleetHandler.HandleAsync(new LockFleetCommand("12345678", "host-1"));
        await lockFleetHandler.HandleAsync(new LockFleetCommand("12345678", "guest-1"));
    }

    private static IReadOnlyList<GameShipPlacement> CreateHostFleet()
    {
        return
        [
            new GameShipPlacement(5, new GameCoordinate(0, 0), ShipOrientation.Horizontal),
            new GameShipPlacement(4, new GameCoordinate(2, 0), ShipOrientation.Horizontal),
            new GameShipPlacement(3, new GameCoordinate(4, 0), ShipOrientation.Horizontal),
            new GameShipPlacement(3, new GameCoordinate(6, 0), ShipOrientation.Horizontal),
            new GameShipPlacement(2, new GameCoordinate(8, 0), ShipOrientation.Horizontal)
        ];
    }

    private static IReadOnlyList<GameShipPlacement> CreateGuestFleet()
    {
        return
        [
            new GameShipPlacement(5, new GameCoordinate(0, 0), ShipOrientation.Vertical),
            new GameShipPlacement(4, new GameCoordinate(0, 2), ShipOrientation.Vertical),
            new GameShipPlacement(3, new GameCoordinate(0, 4), ShipOrientation.Vertical),
            new GameShipPlacement(3, new GameCoordinate(0, 6), ShipOrientation.Vertical),
            new GameShipPlacement(2, new GameCoordinate(0, 8), ShipOrientation.Vertical)
        ];
    }

    private sealed class StubGameCodeGenerator(string gameCode) : IGameCodeGenerator
    {
        public string GenerateCode() => gameCode;
    }

    private sealed class NullEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvents.IntegrationEvent
            => Task.CompletedTask;
    }
}
