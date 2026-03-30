using Dapr.Client;
using HexMaster.BattleShip.Profiles.Abstractions.DomainModels;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.DomainModels;
using HexMaster.BattleShip.Profiles.Persistence;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles;

public sealed class DaprAnonymousPlayerSessionRepository(
    DaprClient daprClient,
    IOptions<AnonymousPlayerSessionOptions> options) : IAnonymousPlayerSessionRepository
{
    private readonly IReadOnlyDictionary<string, string> ttlMetadata = new Dictionary<string, string>
    {
        ["ttlInSeconds"] = ((int)options.Value.PlayerRecordTimeToLive.TotalSeconds).ToString()
    };

    public async Task SaveAsync(IAnonymousPlayerSession session, CancellationToken cancellationToken = default)
    {
        if (session is not AnonymousPlayerSession anonymousPlayerSession)
        {
            throw new InvalidOperationException(
                $"Unsupported domain model type '{session.GetType().FullName}' for anonymous player session persistence.");
        }

        if (!anonymousPlayerSession.HasChanges)
        {
            return;
        }

        await daprClient.SaveStateAsync(
            options.Value.StateStoreName,
            GetStateKey(anonymousPlayerSession.PlayerId),
            anonymousPlayerSession.ToDocument(),
            metadata: ttlMetadata,
            cancellationToken: cancellationToken);

        anonymousPlayerSession.AcceptChanges();
    }

    public async Task<IAnonymousPlayerSession?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var document = await daprClient.GetStateAsync<AnonymousPlayerSessionDocument?>(
            options.Value.StateStoreName,
            GetStateKey(playerId),
            cancellationToken: cancellationToken);

        return document is null
            ? null
            : AnonymousPlayerSession.Rehydrate(document);
    }

    private static string GetStateKey(string playerId) => $"anonymous-player:{playerId}";
}
