using System.Collections.Concurrent;
using HexMaster.BattleShip.Games.Abstractions.DomainModels;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;

namespace HexMaster.BattleShip.Games;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, Persistence.GameDocument> games = new(StringComparer.Ordinal);

    // Per-game semaphores to serialize the entire read-modify-save cycle and prevent lost updates.
    private readonly ConcurrentDictionary<string, SemaphoreSlim> perGameLocks = new(StringComparer.Ordinal);

    public async Task<IAsyncDisposable> BeginUpdateAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        var semaphore = perGameLocks.GetOrAdd(gameCode, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new SemaphoreReleaser(semaphore);
    }

    public Task SaveAsync(IGame game, CancellationToken cancellationToken = default)
    {
        if (game is not Game concreteGame)
        {
            throw new InvalidOperationException($"Unsupported domain model type '{game.GetType().FullName}' for game persistence.");
        }

        if (!concreteGame.HasChanges)
        {
            return Task.CompletedTask;
        }

        games[concreteGame.GameCode] = concreteGame.ToDocument();
        concreteGame.AcceptChanges();
        return Task.CompletedTask;
    }

    public Task<IGame?> GetByCodeAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IGame?>(
            games.TryGetValue(gameCode, out var document)
                ? Game.Rehydrate(document)
                : null);
    }

    private sealed class SemaphoreReleaser(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
