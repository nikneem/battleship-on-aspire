using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
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
        using var activity = ProfilesTelemetry.Source.StartActivity("CreateAnonymousPlayerSession");
        activity?.SetTag("player.name", command.PlayerName);

        try
        {
            var createdAtUtc = timeProvider.GetUtcNow();
            var session = AnonymousPlayerSession.Create(
                command.PlayerName,
                createdAtUtc,
                options.Value.PlayerRecordTimeToLive);

            await sessionRepository.SaveAsync(session, cancellationToken);

            var result = tokenIssuer.IssueToken(session, createdAtUtc);

            activity?.SetTag("player.id", result.PlayerId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            ProfilesTelemetry.SessionsCreated.Add(1);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
