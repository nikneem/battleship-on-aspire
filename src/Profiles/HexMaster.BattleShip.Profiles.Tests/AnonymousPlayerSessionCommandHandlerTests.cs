using System.IdentityModel.Tokens.Jwt;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Commands;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.Models;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles.Tests;

public sealed class AnonymousPlayerSessionCommandHandlerTests
{
    [Fact]
    public async Task CreateAnonymousPlayerSessionStoresRecordAndIssuesJwt()
    {
        var timeProvider = new StubTimeProvider(new DateTimeOffset(2026, 03, 30, 7, 0, 0, TimeSpan.Zero));
        var options = CreateOptions();
        var store = new InMemoryAnonymousPlayerSessionStore();
        var tokenIssuer = new JwtAnonymousPlayerTokenIssuer(options);
        var handler = new CreateAnonymousPlayerSessionCommandHandler(store, tokenIssuer, options, timeProvider);

        var response = await handler.HandleAsync(new CreateAnonymousPlayerSessionCommand("  Eduard  "));
        var storedRecord = await store.GetByIdAsync(response.PlayerId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);

        Assert.NotNull(storedRecord);
        Assert.Equal("Eduard", storedRecord.PlayerName);
        Assert.Equal(timeProvider.GetUtcNow().AddHours(1), storedRecord.ExpiresAtUtc);
        Assert.Equal(response.PlayerId, jwt.Subject);
        Assert.Equal("Eduard", jwt.Claims.Single(claim => claim.Type == AnonymousPlayerClaimNames.PlayerName).Value);
        Assert.Equal(timeProvider.GetUtcNow().AddMinutes(15), response.ExpiresAtUtc);
    }

    [Fact]
    public async Task RenewAnonymousPlayerSessionReissuesTokenForActivePlayer()
    {
        var timeProvider = new StubTimeProvider(new DateTimeOffset(2026, 03, 30, 7, 0, 0, TimeSpan.Zero));
        var options = CreateOptions();
        var store = new InMemoryAnonymousPlayerSessionStore();
        var tokenIssuer = new JwtAnonymousPlayerTokenIssuer(options);
        var createHandler = new CreateAnonymousPlayerSessionCommandHandler(store, tokenIssuer, options, timeProvider);
        var createdSession = await createHandler.HandleAsync(new CreateAnonymousPlayerSessionCommand("Nik"));

        timeProvider.SetUtcNow(createdSession.ExpiresAtUtc.Subtract(TimeSpan.FromMinutes(3)));

        var renewHandler = new RenewAnonymousPlayerSessionCommandHandler(store, tokenIssuer, options, timeProvider);
        var renewedSession = await renewHandler.HandleAsync(
            new RenewAnonymousPlayerSessionCommand(
                createdSession.PlayerId,
                createdSession.PlayerName,
                createdSession.ExpiresAtUtc));

        Assert.Equal(createdSession.PlayerId, renewedSession.PlayerId);
        Assert.Equal(createdSession.PlayerName, renewedSession.PlayerName);
        Assert.True(renewedSession.ExpiresAtUtc > createdSession.ExpiresAtUtc);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(renewedSession.AccessToken);
        Assert.Equal(createdSession.PlayerId, jwt.Subject);
    }

    [Fact]
    public async Task RenewAnonymousPlayerSessionFailsWhenBackingRecordIsMissing()
    {
        var timeProvider = new StubTimeProvider(new DateTimeOffset(2026, 03, 30, 7, 0, 0, TimeSpan.Zero));
        var options = CreateOptions();
        var renewHandler = new RenewAnonymousPlayerSessionCommandHandler(
            new InMemoryAnonymousPlayerSessionStore(),
            new JwtAnonymousPlayerTokenIssuer(options),
            options,
            timeProvider);

        var exception = await Assert.ThrowsAsync<AnonymousPlayerSessionRenewalException>(() =>
            renewHandler.HandleAsync(
                new RenewAnonymousPlayerSessionCommand(
                    "missing-player",
                    "Nik",
                    timeProvider.GetUtcNow().AddMinutes(3))));

        Assert.Equal(AnonymousPlayerSessionRenewalFailureReason.SessionNotFound, exception.Reason);
    }

    private static IOptions<AnonymousPlayerSessionOptions> CreateOptions()
    {
        return Options.Create(new AnonymousPlayerSessionOptions
        {
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            Audience = "HexMaster.BattleShip.App",
            Issuer = "HexMaster.BattleShip.Api",
            JwtSigningKey = "development-only-anonymous-session-signing-key",
            PlayerRecordTimeToLive = TimeSpan.FromHours(1),
            RenewalWindow = TimeSpan.FromMinutes(5),
            StateStoreName = "statestore"
        });
    }

    private sealed class InMemoryAnonymousPlayerSessionStore : IAnonymousPlayerSessionStore
    {
        private readonly Dictionary<string, AnonymousPlayerRecord> records = [];

        public Task SaveAsync(AnonymousPlayerRecord record, CancellationToken cancellationToken = default)
        {
            records[record.PlayerId] = record;
            return Task.CompletedTask;
        }

        public Task<AnonymousPlayerRecord?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default)
        {
            records.TryGetValue(playerId, out var record);
            return Task.FromResult(record);
        }
    }

    private sealed class StubTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => utcNow;

        public void SetUtcNow(DateTimeOffset value) => utcNow = value;
    }
}
