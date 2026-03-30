namespace HexMaster.BattleShip.Profiles.Persistence;

internal sealed record AnonymousPlayerSessionDocument(
    string PlayerId,
    string PlayerName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset LastRenewedAtUtc);
