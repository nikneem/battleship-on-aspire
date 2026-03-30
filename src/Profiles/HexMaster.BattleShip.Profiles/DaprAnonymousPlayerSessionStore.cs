using Dapr.Client;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.Models;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles;

public sealed class DaprAnonymousPlayerSessionStore(
    DaprClient daprClient,
    IOptions<AnonymousPlayerSessionOptions> options) : IAnonymousPlayerSessionStore
{
    private readonly IReadOnlyDictionary<string, string> ttlMetadata = new Dictionary<string, string>
    {
        ["ttlInSeconds"] = ((int)options.Value.PlayerRecordTimeToLive.TotalSeconds).ToString()
    };

    public Task SaveAsync(AnonymousPlayerRecord record, CancellationToken cancellationToken = default)
    {
        return daprClient.SaveStateAsync(
            options.Value.StateStoreName,
            GetStateKey(record.PlayerId),
            record,
            metadata: ttlMetadata,
            cancellationToken: cancellationToken);
    }

    public Task<AnonymousPlayerRecord?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default)
    {
        return daprClient.GetStateAsync<AnonymousPlayerRecord?>(
            options.Value.StateStoreName,
            GetStateKey(playerId),
            cancellationToken: cancellationToken);
    }

    private static string GetStateKey(string playerId) => $"anonymous-player:{playerId}";
}
