using HexMaster.BattleShip.Core;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.DomainModels;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles.Features.CreateAnonymousPlayerSession;

public sealed class CreateAnonymousPlayerSessionHandler(
    IAnonymousPlayerSessionRepository sessionRepository,
    IAnonymousPlayerTokenIssuer tokenIssuer,
    IOptions<AnonymousPlayerSessionOptions> options,
    TimeProvider timeProvider) : ICommandHandler<CreateAnonymousPlayerSessionCommand, AnonymousPlayerSessionResponseDto>
{
    public async Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        CreateAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var createdAtUtc = timeProvider.GetUtcNow();
        var session = AnonymousPlayerSession.Create(
            command.PlayerName,
            createdAtUtc,
            options.Value.PlayerRecordTimeToLive);

        await sessionRepository.SaveAsync(session, cancellationToken);

        return tokenIssuer.IssueToken(session, createdAtUtc);
    }
}
