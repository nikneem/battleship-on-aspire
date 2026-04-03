using HexMaster.BattleShip.Games.Abstractions.DomainModels;

namespace HexMaster.BattleShip.Games.Abstractions.Services;

public interface IGameRepository
{
    /// <summary>
    /// Acquires an exclusive per-game lock for the duration of a read-modify-save cycle.
    /// Dispose the returned handle to release the lock.
    /// All mutating handlers MUST call this before <see cref="GetByCodeAsync"/> to prevent
    /// lost-update race conditions in concurrent request scenarios.
    /// </summary>
    Task<IAsyncDisposable> BeginUpdateAsync(string gameCode, CancellationToken cancellationToken = default);

    Task SaveAsync(IGame game, CancellationToken cancellationToken = default);

    Task<IGame?> GetByCodeAsync(string gameCode, CancellationToken cancellationToken = default);
}

public interface IGameSecretHasher
{
    string HashSecret(string secret);

    bool VerifySecret(string secret, string storedHash);
}

public interface IGameCodeGenerator
{
    string GenerateCode();
}

public interface IRandomProvider
{
    bool NextBool();
}
