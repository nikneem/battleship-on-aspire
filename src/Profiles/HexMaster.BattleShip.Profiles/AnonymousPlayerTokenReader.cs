using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.BattleShip.Profiles;

public sealed class AnonymousPlayerTokenReader(
    IOptions<AnonymousPlayerSessionOptions> options,
    TimeProvider timeProvider)
{
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

    public AnonymousPlayerTokenPayload Read(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw InvalidSession();
        }

        try
        {
            var principal = JwtSecurityTokenHandler.ValidateToken(
                accessToken,
                new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(options.Value.JwtSigningKey)),
                    LifetimeValidator = (notBefore, expires, _, _) =>
                    {
                        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

                        if (notBefore.HasValue && nowUtc < notBefore.Value)
                        {
                            return false;
                        }

                        return !expires.HasValue || nowUtc <= expires.Value;
                    },
                    NameClaimType = AnonymousPlayerClaimNames.PlayerName,
                    ValidAudience = options.Value.Audience,
                    ValidIssuer = options.Value.Issuer,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true
                },
                out var validatedToken);

            var playerId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var playerName = principal.FindFirst(AnonymousPlayerClaimNames.PlayerName)?.Value;

            if (string.IsNullOrWhiteSpace(playerId) ||
                string.IsNullOrWhiteSpace(playerName))
            {
                throw InvalidSession();
            }

            return new AnonymousPlayerTokenPayload(
                playerId,
                playerName,
                new DateTimeOffset(DateTime.SpecifyKind(validatedToken.ValidTo, DateTimeKind.Utc)));
        }
        catch (SecurityTokenException)
        {
            throw InvalidSession();
        }
        catch (ArgumentException)
        {
            throw InvalidSession();
        }
    }

    private static AnonymousPlayerSessionRenewalException InvalidSession()
    {
        return new AnonymousPlayerSessionRenewalException(
            "The anonymous player session is invalid.",
            AnonymousPlayerSessionRenewalFailureReason.InvalidSession);
    }
}

public sealed record AnonymousPlayerTokenPayload(
    string PlayerId,
    string PlayerName,
    DateTimeOffset ExpiresAtUtc);
