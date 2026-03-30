using HexMaster.BattleShip.Profiles.Abstractions.DomainModels;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using HexMaster.BattleShip.Profiles.Persistence;

namespace HexMaster.BattleShip.Profiles.DomainModels;

public sealed class AnonymousPlayerSession : IAnonymousPlayerSession
{
    private AnonymousPlayerSession(
        string playerId,
        string playerName,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset lastRenewedAtUtc,
        bool hasChanges)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        LastRenewedAtUtc = lastRenewedAtUtc;
        HasChanges = hasChanges;
    }

    public string PlayerId { get; }

    public string PlayerName { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset ExpiresAtUtc { get; }

    public DateTimeOffset LastRenewedAtUtc { get; private set; }

    public bool HasChanges { get; private set; }

    public static AnonymousPlayerSession Create(string playerName, DateTimeOffset createdAtUtc, TimeSpan timeToLive)
    {
        if (timeToLive <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeToLive), "Time to live must be greater than zero.");
        }

        return new AnonymousPlayerSession(
            Guid.NewGuid().ToString("N"),
            NormalizePlayerName(playerName),
            createdAtUtc,
            createdAtUtc.Add(timeToLive),
            createdAtUtc,
            hasChanges: true);
    }

    internal static AnonymousPlayerSession Rehydrate(AnonymousPlayerSessionDocument document)
    {
        return new AnonymousPlayerSession(
            document.PlayerId,
            document.PlayerName,
            document.CreatedAtUtc,
            document.ExpiresAtUtc,
            document.LastRenewedAtUtc,
            hasChanges: false);
    }

    public void Renew(
        string playerName,
        DateTimeOffset currentTokenExpiresAtUtc,
        DateTimeOffset nowUtc,
        TimeSpan renewalWindow)
    {
        if (renewalWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(renewalWindow), "Renewal window must be greater than zero.");
        }

        if (ExpiresAtUtc <= nowUtc)
        {
            throw new AnonymousPlayerSessionRenewalException(
                "The anonymous player session has expired and must be recreated.",
                AnonymousPlayerSessionRenewalFailureReason.SessionNotFound);
        }

        if (!string.Equals(PlayerName, playerName, StringComparison.Ordinal))
        {
            throw new AnonymousPlayerSessionRenewalException(
                "The authenticated anonymous player session is invalid.",
                AnonymousPlayerSessionRenewalFailureReason.InvalidSession);
        }

        var remainingLifetime = currentTokenExpiresAtUtc - nowUtc;

        if (remainingLifetime > renewalWindow)
        {
            throw new AnonymousPlayerSessionRenewalException(
                "Anonymous player sessions can only be renewed near token expiry.",
                AnonymousPlayerSessionRenewalFailureReason.TooEarly);
        }

        LastRenewedAtUtc = nowUtc;
        HasChanges = true;
    }

    internal AnonymousPlayerSessionDocument ToDocument()
    {
        return new AnonymousPlayerSessionDocument(
            PlayerId,
            PlayerName,
            CreatedAtUtc,
            ExpiresAtUtc,
            LastRenewedAtUtc);
    }

    internal void AcceptChanges() => HasChanges = false;

    private static string NormalizePlayerName(string playerName)
    {
        var normalizedPlayerName = playerName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedPlayerName))
        {
            throw new ArgumentException("Player name is required.", nameof(playerName));
        }

        if (normalizedPlayerName.Length > AnonymousPlayerSessionConstraints.MaxPlayerNameLength)
        {
            throw new ArgumentException(
                $"Player name must be {AnonymousPlayerSessionConstraints.MaxPlayerNameLength} characters or fewer.",
                nameof(playerName));
        }

        return normalizedPlayerName;
    }
}
