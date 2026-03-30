using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Models;

namespace HexMaster.BattleShip.Profiles.Abstractions.Services;

public interface IAnonymousPlayerSessionStore
{
    Task SaveAsync(AnonymousPlayerRecord record, CancellationToken cancellationToken = default);

    Task<AnonymousPlayerRecord?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default);
}

public interface IAnonymousPlayerTokenIssuer
{
    AnonymousPlayerSessionResponseDto IssueToken(AnonymousPlayerRecord record, DateTimeOffset issuedAtUtc);
}
