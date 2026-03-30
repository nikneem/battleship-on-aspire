namespace HexMaster.BattleShip.Profiles.Abstractions.Models;

public sealed record AnonymousPlayerRecord(
    string PlayerId,
    string PlayerName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
