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
using HexMaster.BattleShip.IntegrationEvents;

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
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);
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
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());

        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9)));
        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "guest-1", new GameCoordinate(9, 8)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9))));
    }

    [Fact]
    public async Task FireShotPublishesGameFinishedEventWhenLastShipIsSunk()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);
        var capturedBus = new CapturingEventBus();
        var fireShotHandler = new FireShotHandler(repository, capturedBus);

        // Guest fleet is 5 vertical ships — 17 cells total; host sinks every one.
        // Guest fires at safe rows (9,x) between each host turn.
        GameCoordinate[] guestFleetCells =
        [
            new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0), // carrier  (col 0, len 5)
            new(0, 2), new(1, 2), new(2, 2), new(3, 2),             // battleship (col 2, len 4)
            new(0, 4), new(1, 4), new(2, 4),                        // cruiser   (col 4, len 3)
            new(0, 6), new(1, 6), new(2, 6),                        // submarine (col 6, len 3)
            new(0, 8), new(1, 8)                                     // destroyer (col 8, len 2)
        ];

        // Guest fires at odd rows (all empty in host fleet) — 16 unique safe cells
        var guestSafeCells = Enumerable
            .Range(0, 16)
            .Select(static i => new GameCoordinate(1 + (i / 10) * 2, i % 10))
            .ToArray();

        for (var i = 0; i < guestFleetCells.Length; i++)
        {
            await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", guestFleetCells[i]));

            // Guest fires back after every shot except the final winning shot
            if (i < guestFleetCells.Length - 1)
            {
                await fireShotHandler.HandleAsync(
                    new FireShotCommand("12345678", "guest-1", guestSafeCells[i]));
            }
        }

        var game = await repository.GetByCodeAsync("12345678");

        Assert.NotNull(game);
        Assert.Equal(GamePhase.Finished, game.Phase);
        Assert.Equal("host-1", game.WinnerPlayerId);
        Assert.Null(game.CurrentTurnPlayerId);
        Assert.Contains(capturedBus.PublishedEvents,
            static e => e is GameFinishedIntegrationEvent finished && finished.WinnerPlayerId == "host-1");
    }

    [Fact]
    public async Task FireShotWinConditionSurvivesRepositoryRoundTrip()
    {
        // This test specifically validates the rehydration bug fix:
        // hit state must be restored from IncomingShots when loading from the repository.
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());

        // Sink the destroyer (len 2) at (0,8) and (1,8) with a guest turn in between.
        // If rehydration is broken, the second hit would not see the first hit and IsSunk stays false.
        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(0, 8)));
        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "guest-1", new GameCoordinate(9, 9)));

        var stateAfterSecondHit = await fireShotHandler.HandleAsync(
            new FireShotCommand("12345678", "host-1", new GameCoordinate(1, 8)));

        // The destroyer should report Sunk (outcome 2), not just Hit (outcome 1)
        var destroyerShot = stateAfterSecondHit.OpponentBoard.KnownShots
            .Single(s => s.Coordinate.Row == 1 && s.Coordinate.Column == 8);
        Assert.Equal(HexMaster.BattleShip.Games.Abstractions.Models.ShotOutcome.Sunk, destroyerShot.Outcome);
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
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);
        var fireShotHandler = new FireShotHandler(repository, new NullEventBus());
        var queryHandler = new GetGameStateForPlayerHandler(repository);

        await fireShotHandler.HandleAsync(new FireShotCommand("12345678", "host-1", new GameCoordinate(9, 9)));

        var state = await queryHandler.HandleAsync(new GetGameStateForPlayerQuery("12345678", "host-1"));

        Assert.Equal(5, state.OwnBoard.Ships.Count);
        Assert.Single(state.OpponentBoard.KnownShots);
        Assert.Equal(new GameCoordinateDto(9, 9), state.OpponentBoard.KnownShots[0].Coordinate);
        Assert.Equal(PlayerGameStateProjection.OpponentTurn, state.CurrentPlayer.State);
    }

    [Fact]
    public async Task LockFleetSetsHostAsFirstTurnWhenRandomProviderReturnsTrue()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: true);

        var game = await repository.GetByCodeAsync("12345678");

        Assert.NotNull(game);
        Assert.Equal(GamePhase.InProgress, game.Phase);
        Assert.Equal("host-1", game.CurrentTurnPlayerId);
    }

    [Fact]
    public async Task LockFleetSetsGuestAsFirstTurnWhenRandomProviderReturnsFalse()
    {
        var repository = new InMemoryGameRepository();
        await CreateReadyToPlayGameAsync(repository, hostGoesFirst: false);

        var game = await repository.GetByCodeAsync("12345678");

        Assert.NotNull(game);
        Assert.Equal(GamePhase.InProgress, game.Phase);
        Assert.Equal("guest-1", game.CurrentTurnPlayerId);
    }

    [Fact]
    public async Task LockFleetFirstLockDoesNotTransitionToInProgress()
    {
        var repository = new InMemoryGameRepository();
        await CreateJoinedLobbyAsync(repository);
        var nullBus = new NullEventBus();
        var markReadyHandler = new MarkReadyHandler(repository, nullBus);
        var submitFleetHandler = new SubmitFleetHandler(repository, nullBus);
        var lockFleetHandler = new LockFleetHandler(repository, nullBus, new FixedRandomProvider(true));

        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "host-1"));
        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "guest-1"));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "host-1", CreateHostFleet()));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "guest-1", CreateGuestFleet()));
        await lockFleetHandler.HandleAsync(new LockFleetCommand("12345678", "host-1"));

        var game = await repository.GetByCodeAsync("12345678");

        Assert.NotNull(game);
        Assert.Equal(GamePhase.Setup, game.Phase);
        Assert.Null(game.CurrentTurnPlayerId);
    }

    [Fact]
    public async Task LockFleetPublishesGameStartedEventOnlyWhenBothFleetsAreLocked()
    {
        var repository = new InMemoryGameRepository();
        await CreateJoinedLobbyAsync(repository);
        var capturedBus = new CapturingEventBus();
        var markReadyHandler = new MarkReadyHandler(repository, capturedBus);
        var submitFleetHandler = new SubmitFleetHandler(repository, capturedBus);
        var lockFleetHandler = new LockFleetHandler(repository, capturedBus, new FixedRandomProvider(true));

        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "host-1"));
        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "guest-1"));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "host-1", CreateHostFleet()));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "guest-1", CreateGuestFleet()));

        capturedBus.PublishedEvents.Clear();

        await lockFleetHandler.HandleAsync(new LockFleetCommand("12345678", "host-1"));
        Assert.DoesNotContain(capturedBus.PublishedEvents, e => e is GameStartedIntegrationEvent);

        await lockFleetHandler.HandleAsync(new LockFleetCommand("12345678", "guest-1"));
        Assert.Contains(capturedBus.PublishedEvents, e => e is GameStartedIntegrationEvent started && started.FirstTurnPlayerId == "host-1");
    }

    [Fact]
    public async Task LockFleetFirstTurnSelectionIsIndependentOfLockOrder()
    {
        var repositoryHostFirst = new InMemoryGameRepository();
        var repositoryGuestFirst = new InMemoryGameRepository();

        await SetupBothLobbyStates(repositoryHostFirst);
        await SetupBothLobbyStates(repositoryGuestFirst);

        var nullBus = new NullEventBus();
        var hostFirstLockHandler = new LockFleetHandler(repositoryHostFirst, nullBus, new FixedRandomProvider(false));
        var guestFirstLockHandler = new LockFleetHandler(repositoryGuestFirst, nullBus, new FixedRandomProvider(false));

        // Host locks first in repo 1; guest locks first in repo 2 — both use FixedRandomProvider(false) → guest always wins
        await hostFirstLockHandler.HandleAsync(new LockFleetCommand("12345678", "host-1"));
        await hostFirstLockHandler.HandleAsync(new LockFleetCommand("12345678", "guest-1"));

        await guestFirstLockHandler.HandleAsync(new LockFleetCommand("12345678", "guest-1"));
        await guestFirstLockHandler.HandleAsync(new LockFleetCommand("12345678", "host-1"));

        var game1 = await repositoryHostFirst.GetByCodeAsync("12345678");
        var game2 = await repositoryGuestFirst.GetByCodeAsync("12345678");

        Assert.Equal("guest-1", game1!.CurrentTurnPlayerId);
        Assert.Equal("guest-1", game2!.CurrentTurnPlayerId);
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

    private static async Task SetupBothLobbyStates(InMemoryGameRepository repository)
    {
        await CreateJoinedLobbyAsync(repository);
        var nullBus = new NullEventBus();
        var markReadyHandler = new MarkReadyHandler(repository, nullBus);
        var submitFleetHandler = new SubmitFleetHandler(repository, nullBus);
        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "host-1"));
        await markReadyHandler.HandleAsync(new MarkReadyCommand("12345678", "guest-1"));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "host-1", CreateHostFleet()));
        await submitFleetHandler.HandleAsync(new SubmitFleetCommand("12345678", "guest-1", CreateGuestFleet()));
    }

    private static async Task CreateReadyToPlayGameAsync(InMemoryGameRepository repository, bool hostGoesFirst)
    {
        await CreateJoinedLobbyAsync(repository);

        var nullBus = new NullEventBus();
        var markReadyHandler = new MarkReadyHandler(repository, nullBus);
        var submitFleetHandler = new SubmitFleetHandler(repository, nullBus);
        var lockFleetHandler = new LockFleetHandler(repository, nullBus, new FixedRandomProvider(hostGoesFirst));

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

    private sealed class FixedRandomProvider(bool value) : IRandomProvider
    {
        public bool NextBool() => value;
    }

    private sealed class NullEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
            => Task.CompletedTask;
    }

    private sealed class CapturingEventBus : IEventBus
    {
        public List<object> PublishedEvents { get; } = [];

        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
        {
            PublishedEvents.Add(integrationEvent!);
            return Task.CompletedTask;
        }
    }
}
