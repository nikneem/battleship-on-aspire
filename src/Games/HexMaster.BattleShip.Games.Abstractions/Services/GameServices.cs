using HexMaster.BattleShip.Games.Abstractions.DomainModels;

namespace HexMaster.BattleShip.Games.Abstractions.Services;

public interface IGameRepository
{
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
