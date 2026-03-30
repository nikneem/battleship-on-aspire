using System.IdentityModel.Tokens.Jwt;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DomainModels;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using HexMaster.BattleShip.Profiles.DomainModels;
using HexMaster.BattleShip.Profiles.Features.CreateAnonymousPlayerSession;
using HexMaster.BattleShip.Profiles.Features.RenewAnonymousPlayerSession;
using Microsoft.Extensions.Options;

namespace HexMaster.BattleShip.Profiles.Tests;

public sealed class AnonymousPlayerSessionCommandHandlerTests
{
    [Fact]
    public async Task CreateAnonymousPlayerSessionStoresRecordAndIssuesJwt()
    {
        var timeProvider = new StubTimeProvider(new DateTimeOffset(2026, 03, 30, 7, 0, 0, TimeSpan.Zero));
        var options = CreateOptions();
        var repository = new InMemoryAnonymousPlayerSessionRepository();
        var tokenIssuer = new JwtAnonymousPlayerTokenIssuer(options);
        var handler = new CreateAnonymousPlayerSessionHandler(repository, tokenIssuer, options, timeProvider);

        var response = await handler.HandleAsync(new CreateAnonymousPlayerSessionCommand("  Eduard  "));
        var storedRecord = await repository.GetByIdAsync(response.PlayerId);
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
        var repository = new InMemoryAnonymousPlayerSessionRepository();
        var tokenIssuer = new JwtAnonymousPlayerTokenIssuer(options);
        var tokenReader = new AnonymousPlayerTokenReader(options, timeProvider);
        var createHandler = new CreateAnonymousPlayerSessionHandler(repository, tokenIssuer, options, timeProvider);
        var createdSession = await createHandler.HandleAsync(new CreateAnonymousPlayerSessionCommand("Nik"));

        timeProvider.SetUtcNow(createdSession.ExpiresAtUtc.Subtract(TimeSpan.FromMinutes(3)));

        var renewHandler = new RenewAnonymousPlayerSessionHandler(repository, tokenIssuer, tokenReader, options, timeProvider);
        var renewedSession = await renewHandler.HandleAsync(
            new RenewAnonymousPlayerSessionCommand(createdSession.AccessToken));

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
        var seedSession = AnonymousPlayerSession.Create("Nik", timeProvider.GetUtcNow(), options.Value.PlayerRecordTimeToLive);
        var tokenIssuer = new JwtAnonymousPlayerTokenIssuer(options);
        var renewHandler = new RenewAnonymousPlayerSessionHandler(
            new InMemoryAnonymousPlayerSessionRepository(),
            tokenIssuer,
            new AnonymousPlayerTokenReader(options, timeProvider),
            options,
            timeProvider);

        var exception = await Assert.ThrowsAsync<AnonymousPlayerSessionRenewalException>(() =>
            renewHandler.HandleAsync(
                new RenewAnonymousPlayerSessionCommand(
                    tokenIssuer.IssueToken(seedSession, timeProvider.GetUtcNow()).AccessToken)));

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

    private sealed class InMemoryAnonymousPlayerSessionRepository : IAnonymousPlayerSessionRepository
    {
        private readonly Dictionary<string, IAnonymousPlayerSession> sessions = [];

        public Task SaveAsync(IAnonymousPlayerSession session, CancellationToken cancellationToken = default)
        {
            sessions[session.PlayerId] = session;
            return Task.CompletedTask;
        }

        public Task<IAnonymousPlayerSession?> GetByIdAsync(string playerId, CancellationToken cancellationToken = default)
        {
            sessions.TryGetValue(playerId, out var session);
            return Task.FromResult(session);
        }
    }

    private sealed class StubTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => utcNow;

        public void SetUtcNow(DateTimeOffset value) => utcNow = value;
    }
}
