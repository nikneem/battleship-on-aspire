using System.Collections.Concurrent;
using HexMaster.BattleShip.Games.Abstractions.DomainModels;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;

namespace HexMaster.BattleShip.Games;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, Persistence.GameDocument> games = new(StringComparer.Ordinal);

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
}
