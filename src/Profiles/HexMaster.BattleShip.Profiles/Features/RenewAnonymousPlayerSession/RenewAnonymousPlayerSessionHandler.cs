using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles.Features.RenewAnonymousPlayerSession;

public sealed class RenewAnonymousPlayerSessionHandler(
    IAnonymousPlayerSessionRepository sessionRepository,
    IAnonymousPlayerTokenIssuer tokenIssuer,
    AnonymousPlayerTokenReader tokenReader,
    IOptions<AnonymousPlayerSessionOptions> options,
    TimeProvider timeProvider) : ICommandHandler<RenewAnonymousPlayerSessionCommand, AnonymousPlayerSessionResponseDto>
{
    public async Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        RenewAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var tokenPayload = tokenReader.Read(command.AccessToken);
        var session = await sessionRepository.GetByIdAsync(tokenPayload.PlayerId, cancellationToken);

        if (session is null)
        {
            throw new AnonymousPlayerSessionRenewalException(
                "The anonymous player session has expired and must be recreated.",
                AnonymousPlayerSessionRenewalFailureReason.SessionNotFound);
        }

        var now = timeProvider.GetUtcNow();
        session.Renew(tokenPayload.PlayerName, tokenPayload.ExpiresAtUtc, now, options.Value.RenewalWindow);
        await sessionRepository.SaveAsync(session, cancellationToken);

        return tokenIssuer.IssueToken(session, now);
    }
}
