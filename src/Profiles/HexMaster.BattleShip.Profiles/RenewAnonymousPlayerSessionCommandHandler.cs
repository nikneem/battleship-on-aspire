using HexMaster.BattleShip.Profiles.Abstractions.Commands;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Handlers;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles;

public sealed class RenewAnonymousPlayerSessionCommandHandler(
    IAnonymousPlayerSessionStore sessionStore,
    IAnonymousPlayerTokenIssuer tokenIssuer,
    IOptions<AnonymousPlayerSessionOptions> options,
    TimeProvider timeProvider) : IRenewAnonymousPlayerSessionCommandHandler
{
    public async Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        RenewAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var remainingLifetime = command.CurrentTokenExpiresAtUtc - now;

        if (remainingLifetime > options.Value.RenewalWindow)
        {
            throw new AnonymousPlayerSessionRenewalException(
                "Anonymous player sessions can only be renewed near token expiry.",
                AnonymousPlayerSessionRenewalFailureReason.TooEarly);
        }

        var record = await sessionStore.GetByIdAsync(command.PlayerId, cancellationToken);

        if (record is null || record.ExpiresAtUtc <= now)
        {
            throw new AnonymousPlayerSessionRenewalException(
                "The anonymous player session has expired and must be recreated.",
                AnonymousPlayerSessionRenewalFailureReason.SessionNotFound);
        }

        if (!string.Equals(record.PlayerName, command.PlayerName, StringComparison.Ordinal))
        {
            throw new AnonymousPlayerSessionRenewalException(
                "The authenticated anonymous player session is invalid.",
                AnonymousPlayerSessionRenewalFailureReason.InvalidSession);
        }

        return tokenIssuer.IssueToken(record, now);
    }
}
