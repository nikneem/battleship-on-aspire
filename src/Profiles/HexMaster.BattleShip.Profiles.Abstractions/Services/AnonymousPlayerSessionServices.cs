using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.DomainModels;

namespace HexMaster.BattleShip.Profiles.Abstractions.Services;

public interface IAnonymousPlayerSessionRepository
{
    Task SaveAsync(IAnonymousPlayerSession session, CancellationToken cancellationToken = default);

    Task<IAnonymousPlayerSession?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default);
}

public interface IAnonymousPlayerTokenIssuer
{
    AnonymousPlayerSessionResponseDto IssueToken(IAnonymousPlayerSession session, DateTimeOffset issuedAtUtc);
}
