using HexMaster.BattleShip.Profiles.Abstractions.Commands;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Handlers;
using HexMaster.BattleShip.Profiles.Abstractions.Models;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles;

public sealed class CreateAnonymousPlayerSessionCommandHandler(
    IAnonymousPlayerSessionStore sessionStore,
    IAnonymousPlayerTokenIssuer tokenIssuer,
    IOptions<AnonymousPlayerSessionOptions> options,
    TimeProvider timeProvider) : ICreateAnonymousPlayerSessionCommandHandler
{
    private const int MaxPlayerNameLength = 40;

    public async Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        CreateAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedPlayerName = NormalizePlayerName(command.PlayerName);
        var createdAtUtc = timeProvider.GetUtcNow();
        var record = new AnonymousPlayerRecord(
            Guid.NewGuid().ToString("N"),
            normalizedPlayerName,
            createdAtUtc,
            createdAtUtc.Add(options.Value.PlayerRecordTimeToLive));

        await sessionStore.SaveAsync(record, cancellationToken);

        return tokenIssuer.IssueToken(record, createdAtUtc);
    }

    private static string NormalizePlayerName(string playerName)
    {
        var normalizedPlayerName = playerName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedPlayerName))
        {
            throw new ArgumentException("Player name is required.", nameof(playerName));
        }

        if (normalizedPlayerName.Length > MaxPlayerNameLength)
        {
            throw new ArgumentException(
                $"Player name must be {MaxPlayerNameLength} characters or fewer.",
                nameof(playerName));
        }

        return normalizedPlayerName;
    }
}
