namespace HexMaster.BattleShip.Profiles.Abstractions.Commands;

public sealed record CreateAnonymousPlayerSessionCommand(string PlayerName);

public sealed record RenewAnonymousPlayerSessionCommand(
    string PlayerId,
    string PlayerName,
    DateTimeOffset CurrentTokenExpiresAtUtc);
