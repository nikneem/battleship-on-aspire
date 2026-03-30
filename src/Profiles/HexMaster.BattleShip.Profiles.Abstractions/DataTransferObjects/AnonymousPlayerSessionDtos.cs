namespace HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;

public sealed record CreateAnonymousPlayerSessionRequestDto(string PlayerName);

public sealed record RenewAnonymousPlayerSessionRequestDto();

public sealed record AnonymousPlayerSessionResponseDto(
    string PlayerId,
    string PlayerName,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);
