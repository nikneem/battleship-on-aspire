using System.ComponentModel.DataAnnotations;

namespace HexMaster.BattleShip.Profiles.Abstractions.Configuration;

public sealed class AnonymousPlayerSessionOptions
{
    public const string SectionName = "AnonymousPlayerSessions";

    [Required]
    public string StateStoreName { get; init; } = "statestore";

    [Required]
    [MinLength(32)]
    public string JwtSigningKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "HexMaster.BattleShip.Api";

    [Required]
    public string Audience { get; init; } = "HexMaster.BattleShip.App";

    public TimeSpan PlayerRecordTimeToLive { get; init; } = TimeSpan.FromHours(1);

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan RenewalWindow { get; init; } = TimeSpan.FromMinutes(5);
}
