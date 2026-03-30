using HexMaster.BattleShip.Games.Abstractions.DomainModels;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Persistence;

namespace HexMaster.BattleShip.Games.DomainModels;

public sealed class Game : IGame
{
    private readonly PlayerSlot host;
    private PlayerSlot? guest;
    private readonly string? protectedSecretHash;

    private Game(
        string gameCode,
        GamePhase phase,
        string? protectedSecretHash,
        string? currentTurnPlayerId,
        string? winnerPlayerId,
        PlayerSlot host,
        PlayerSlot? guest,
        bool hasChanges)
    {
        GameCode = NormalizeGameCode(gameCode);
        Phase = phase;
        this.protectedSecretHash = string.IsNullOrWhiteSpace(protectedSecretHash) ? null : protectedSecretHash;
        CurrentTurnPlayerId = currentTurnPlayerId;
        WinnerPlayerId = winnerPlayerId;
        this.host = host;
        this.guest = guest;
        HasChanges = hasChanges;
    }

    public string GameCode { get; }

    public GamePhase Phase { get; private set; }

    public bool IsProtected => protectedSecretHash is not null;

    public string? CurrentTurnPlayerId { get; private set; }

    public string? WinnerPlayerId { get; private set; }

    public bool HasChanges { get; private set; }

    public IGamePlayerSlot Host => host;

    public IGamePlayerSlot? Guest => guest;

    public static Game Create(
        string hostPlayerId,
        string hostPlayerName,
        string gameCode,
        string? protectedSecretHash = null)
    {
        return new Game(
            gameCode,
            GamePhase.LobbyOpen,
            protectedSecretHash,
            currentTurnPlayerId: null,
            winnerPlayerId: null,
            PlayerSlot.Create(hostPlayerId, hostPlayerName),
            guest: null,
            hasChanges: true);
    }

    internal static Game Rehydrate(GameDocument document)
    {
        return new Game(
            document.GameCode,
            document.Phase,
            document.ProtectedSecretHash,
            document.CurrentTurnPlayerId,
            document.WinnerPlayerId,
            PlayerSlot.Rehydrate(document.Host),
            document.Guest is null ? null : PlayerSlot.Rehydrate(document.Guest),
            hasChanges: false);
    }

    public void JoinGuest(string playerId, string playerName, bool secretValidated)
    {
        EnsurePhase(GamePhase.LobbyOpen);

        if (guest is not null)
        {
            throw new InvalidOperationException("The game already has a guest player.");
        }

        if (string.Equals(host.PlayerId, playerId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The host cannot join their own game as a guest.");
        }

        if (IsProtected && !secretValidated)
        {
            throw new InvalidOperationException("A valid join secret is required to join this game.");
        }

        guest = PlayerSlot.Create(playerId, playerName);
        Phase = GamePhase.LobbyFull;
        HasChanges = true;
    }

    public void MarkReady(string playerId)
    {
        EnsurePhase(GamePhase.LobbyFull);

        var player = RequireJoinedPlayer(playerId);
        player.MarkReady();

        if (host.IsReady && guest?.IsReady == true)
        {
            Phase = GamePhase.Setup;
        }

        HasChanges = true;
    }

    public void SubmitFleet(string playerId, IReadOnlyCollection<GameShipPlacement> ships)
    {
        EnsurePhase(GamePhase.Setup);
        RequireJoinedPlayer(playerId).Board.SubmitFleet(ships);
        HasChanges = true;
    }

    public void LockFleet(string playerId)
    {
        EnsurePhase(GamePhase.Setup);

        var player = RequireJoinedPlayer(playerId);
        player.Board.Lock();

        if (host.Board.IsLocked && guest?.Board.IsLocked == true)
        {
            Phase = GamePhase.InProgress;
            CurrentTurnPlayerId = host.PlayerId;
        }

        HasChanges = true;
    }

    public ShotOutcome FireShot(string playerId, GameCoordinate target)
    {
        EnsurePhase(GamePhase.InProgress);

        if (!string.Equals(CurrentTurnPlayerId, playerId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only the player whose turn it is may fire a shot.");
        }

        var opponent = RequireOpponent(playerId);
        var outcome = opponent.Board.ReceiveShot(target);

        if (opponent.Board.AllShipsSunk)
        {
            Phase = GamePhase.Finished;
            WinnerPlayerId = playerId;
            CurrentTurnPlayerId = null;
        }
        else
        {
            CurrentTurnPlayerId = opponent.PlayerId;
        }

        HasChanges = true;
        return outcome;
    }

    public void Cancel(string playerId)
    {
        EnsurePlayerIsHost(playerId);

        if (Phase is not (GamePhase.LobbyOpen or GamePhase.LobbyFull))
        {
            throw new InvalidOperationException("Only joinable lobbies can be cancelled.");
        }

        Phase = GamePhase.Cancelled;
        CurrentTurnPlayerId = null;
        WinnerPlayerId = null;
        HasChanges = true;
    }

    public void Abandon(string playerId)
    {
        _ = RequireJoinedPlayer(playerId);

        if (Phase is not (GamePhase.LobbyFull or GamePhase.Setup or GamePhase.InProgress))
        {
            throw new InvalidOperationException("Only joined or active games can be abandoned.");
        }

        Phase = GamePhase.Abandoned;
        CurrentTurnPlayerId = null;
        WinnerPlayerId = null;
        HasChanges = true;
    }

    internal GameDocument ToDocument()
    {
        return new GameDocument(
            GameCode,
            Phase,
            protectedSecretHash,
            CurrentTurnPlayerId,
            WinnerPlayerId,
            host.ToDocument(),
            guest?.ToDocument());
    }

    internal void AcceptChanges() => HasChanges = false;

    private PlayerSlot RequireJoinedPlayer(string playerId)
    {
        if (string.Equals(host.PlayerId, playerId, StringComparison.Ordinal))
        {
            return host;
        }

        if (guest is not null && string.Equals(guest.PlayerId, playerId, StringComparison.Ordinal))
        {
            return guest;
        }

        throw new UnauthorizedAccessException("Only joined players can perform this action.");
    }

    private PlayerSlot RequireOpponent(string playerId)
    {
        if (string.Equals(host.PlayerId, playerId, StringComparison.Ordinal))
        {
            return guest ?? throw new InvalidOperationException("A guest player has not joined this game yet.");
        }

        if (guest is not null && string.Equals(guest.PlayerId, playerId, StringComparison.Ordinal))
        {
            return host;
        }

        throw new UnauthorizedAccessException("Only joined players can perform this action.");
    }

    private void EnsurePlayerIsHost(string playerId)
    {
        if (!string.Equals(host.PlayerId, playerId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Only the host player can perform this action.");
        }
    }

    private void EnsurePhase(GamePhase expectedPhase)
    {
        if (Phase != expectedPhase)
        {
            throw new InvalidOperationException($"This action is only allowed while the game is in the {expectedPhase} phase.");
        }
    }

    private static string NormalizeGameCode(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            throw new ArgumentException("Game code is required.", nameof(gameCode));
        }

        var normalizedCode = gameCode.Trim();

        if (normalizedCode.Length != GameConstraints.GameCodeLength || normalizedCode.Any(static character => !char.IsDigit(character)))
        {
            throw new ArgumentException(
                $"Game code must be exactly {GameConstraints.GameCodeLength} digits.",
                nameof(gameCode));
        }

        return normalizedCode;
    }
}

internal sealed class PlayerSlot : IGamePlayerSlot
{
    private PlayerSlot(string playerId, string playerName, bool isReady, PlayerBoard board)
    {
        PlayerId = NormalizePlayerId(playerId);
        PlayerName = NormalizePlayerName(playerName);
        IsReady = isReady;
        Board = board;
    }

    public string PlayerId { get; }

    public string PlayerName { get; }

    public bool IsReady { get; private set; }

    public PlayerBoard Board { get; }

    IPlayerBoard IGamePlayerSlot.Board => Board;

    public static PlayerSlot Create(string playerId, string playerName)
    {
        return new PlayerSlot(playerId, playerName, isReady: false, PlayerBoard.Create());
    }

    internal static PlayerSlot Rehydrate(PlayerSlotDocument document)
    {
        return new PlayerSlot(
            document.PlayerId,
            document.PlayerName,
            document.IsReady,
            PlayerBoard.Rehydrate(document.Board));
    }

    public void MarkReady() => IsReady = true;

    internal PlayerSlotDocument ToDocument() => new(PlayerId, PlayerName, IsReady, Board.ToDocument());

    private static string NormalizePlayerId(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new ArgumentException("Player identifier is required.", nameof(playerId));
        }

        return playerId.Trim();
    }

    private static string NormalizePlayerName(string playerName)
    {
        var normalizedPlayerName = playerName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedPlayerName))
        {
            throw new ArgumentException("Player name is required.", nameof(playerName));
        }

        if (normalizedPlayerName.Length > GameConstraints.MaxPlayerNameLength)
        {
            throw new ArgumentException(
                $"Player name must be {GameConstraints.MaxPlayerNameLength} characters or fewer.",
                nameof(playerName));
        }

        return normalizedPlayerName;
    }
}

internal sealed class PlayerBoard : IPlayerBoard
{
    private readonly List<PlacedShip> ships;
    private readonly List<ShotRecord> incomingShots;

    private PlayerBoard(bool isLocked, IEnumerable<PlacedShip>? ships = null, IEnumerable<ShotRecord>? incomingShots = null)
    {
        IsLocked = isLocked;
        this.ships = ships?.ToList() ?? [];
        this.incomingShots = incomingShots?.ToList() ?? [];
    }

    public bool HasSubmittedFleet => ships.Count > 0;

    public bool IsLocked { get; private set; }

    public bool AllShipsSunk => ships.Count > 0 && ships.All(static ship => ship.IsSunk);

    public IReadOnlyList<GameShipPlacement> Ships => ships.Select(static ship => ship.ToContract()).ToArray();

    public IReadOnlyList<GameShotRecord> IncomingShots => incomingShots.Select(static shot => shot.ToContract()).ToArray();

    public static PlayerBoard Create() => new(isLocked: false);

    internal static PlayerBoard Rehydrate(BoardDocument document)
    {
        return new PlayerBoard(
            document.IsLocked,
            document.Ships.Select(static placement => PlacedShip.FromContract(placement)),
            document.IncomingShots.Select(static shot => ShotRecord.FromContract(shot)));
    }

    public void SubmitFleet(IReadOnlyCollection<GameShipPlacement> placements)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("Locked fleets cannot be modified.");
        }

        ArgumentNullException.ThrowIfNull(placements);

        var normalizedPlacements = placements
            .Select(static placement => PlacedShip.FromContract(placement))
            .OrderBy(static ship => ship.Length)
            .ToArray();
        var requiredShipLengths = GameConstraints.RequiredShipLengths.OrderBy(static length => length).ToArray();
        var suppliedShipLengths = normalizedPlacements.Select(static ship => ship.Length).OrderBy(static length => length).ToArray();

        if (!requiredShipLengths.SequenceEqual(suppliedShipLengths))
        {
            throw new InvalidOperationException("Submitted fleets must contain the standard Battleship ship sizes.");
        }

        foreach (var ship in normalizedPlacements)
        {
            ship.ValidateBounds();
        }

        for (var index = 0; index < normalizedPlacements.Length; index += 1)
        {
            for (var otherIndex = index + 1; otherIndex < normalizedPlacements.Length; otherIndex += 1)
            {
                if (normalizedPlacements[index].Overlaps(normalizedPlacements[otherIndex]))
                {
                    throw new InvalidOperationException("Submitted fleets must not contain overlapping ships.");
                }
            }
        }

        ships.Clear();
        ships.AddRange(normalizedPlacements);
        incomingShots.Clear();
    }

    public void Lock()
    {
        if (!HasSubmittedFleet)
        {
            throw new InvalidOperationException("A fleet must be submitted before it can be locked.");
        }

        IsLocked = true;
    }

    public ShotOutcome ReceiveShot(GameCoordinate coordinate)
    {
        ValidateCoordinate(coordinate);

        if (incomingShots.Any(existingShot => existingShot.Coordinate == coordinate))
        {
            throw new InvalidOperationException("The selected coordinate has already been targeted.");
        }

        var targetShip = ships.SingleOrDefault(ship => ship.Occupies(coordinate));

        if (targetShip is null)
        {
            var miss = ShotRecord.Create(coordinate, ShotOutcome.Miss);
            incomingShots.Add(miss);
            return miss.Outcome;
        }

        targetShip.ApplyHit(coordinate);
        var outcome = targetShip.IsSunk ? ShotOutcome.Sunk : ShotOutcome.Hit;
        incomingShots.Add(ShotRecord.Create(coordinate, outcome));
        return outcome;
    }

    internal BoardDocument ToDocument() => new(IsLocked, Ships, IncomingShots);

    private static void ValidateCoordinate(GameCoordinate coordinate)
    {
        if (coordinate.Row < 0 ||
            coordinate.Row >= GameConstraints.BoardSize ||
            coordinate.Column < 0 ||
            coordinate.Column >= GameConstraints.BoardSize)
        {
            throw new InvalidOperationException("Shot coordinates must remain within the board bounds.");
        }
    }
}

internal sealed class PlacedShip
{
    private readonly HashSet<GameCoordinate> hitCoordinates = [];

    private PlacedShip(int length, GameCoordinate start, ShipOrientation orientation)
    {
        Length = length;
        Start = start;
        Orientation = orientation;
    }

    public int Length { get; }

    public GameCoordinate Start { get; }

    public ShipOrientation Orientation { get; }

    public bool IsSunk => Coordinates().All(hitCoordinates.Contains);

    public static PlacedShip FromContract(GameShipPlacement placement)
    {
        if (placement.Length <= 0)
        {
            throw new InvalidOperationException("Ship length must be greater than zero.");
        }

        return new PlacedShip(placement.Length, placement.Start, placement.Orientation);
    }

    public GameShipPlacement ToContract() => new(Length, Start, Orientation);

    public bool Occupies(GameCoordinate coordinate) => Coordinates().Contains(coordinate);

    public bool Overlaps(PlacedShip other) => Coordinates().Intersect(other.Coordinates()).Any();

    public void ApplyHit(GameCoordinate coordinate)
    {
        if (!Occupies(coordinate))
        {
            throw new InvalidOperationException("Only occupied coordinates can be hit.");
        }

        hitCoordinates.Add(coordinate);
    }

    public void ValidateBounds()
    {
        foreach (var coordinate in Coordinates())
        {
            if (coordinate.Row < 0 ||
                coordinate.Row >= GameConstraints.BoardSize ||
                coordinate.Column < 0 ||
                coordinate.Column >= GameConstraints.BoardSize)
            {
                throw new InvalidOperationException("Ship placements must remain within the board bounds.");
            }
        }
    }

    public IReadOnlyList<GameCoordinate> Coordinates()
    {
        return Enumerable
            .Range(0, Length)
            .Select(offset => Orientation == ShipOrientation.Horizontal
                ? new GameCoordinate(Start.Row, Start.Column + offset)
                : new GameCoordinate(Start.Row + offset, Start.Column))
            .ToArray();
    }
}

internal sealed class ShotRecord(GameCoordinate coordinate, ShotOutcome outcome)
{
    public GameCoordinate Coordinate { get; } = coordinate;

    public ShotOutcome Outcome { get; } = outcome;

    public static ShotRecord Create(GameCoordinate coordinate, ShotOutcome outcome) => new(coordinate, outcome);

    public static ShotRecord FromContract(GameShotRecord shotRecord) => new(shotRecord.Coordinate, shotRecord.Outcome);

    public GameShotRecord ToContract() => new(Coordinate, Outcome);
}
