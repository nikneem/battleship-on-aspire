namespace HexMaster.BattleShip.Profiles.Abstractions.DomainModels;

public interface IAnonymousPlayerSession
{
    string PlayerId { get; }

    string PlayerName { get; }

    DateTimeOffset CreatedAtUtc { get; }

    DateTimeOffset ExpiresAtUtc { get; }

    DateTimeOffset LastRenewedAtUtc { get; }

    bool HasChanges { get; }

    void Renew(
        string playerName,
        DateTimeOffset currentTokenExpiresAtUtc,
        DateTimeOffset nowUtc,
        TimeSpan renewalWindow);
}
