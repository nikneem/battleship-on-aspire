using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Models;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.BattleShip.Profiles;

public sealed class JwtAnonymousPlayerTokenIssuer(
    IOptions<AnonymousPlayerSessionOptions> options) : IAnonymousPlayerTokenIssuer
{
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

    public AnonymousPlayerSessionResponseDto IssueToken(AnonymousPlayerRecord record, DateTimeOffset issuedAtUtc)
    {
        var requestedExpiration = issuedAtUtc.Add(options.Value.AccessTokenLifetime);
        var expiresAtUtc = requestedExpiration <= record.ExpiresAtUtc
            ? requestedExpiration
            : record.ExpiresAtUtc;

        var identity = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, record.PlayerId),
            new Claim(AnonymousPlayerClaimNames.PlayerName, record.PlayerName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        ]);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.JwtSigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = JwtSecurityTokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Audience = options.Value.Audience,
            Expires = expiresAtUtc.UtcDateTime,
            IssuedAt = issuedAtUtc.UtcDateTime,
            Issuer = options.Value.Issuer,
            NotBefore = issuedAtUtc.UtcDateTime,
            SigningCredentials = signingCredentials,
            Subject = identity
        });

        return new AnonymousPlayerSessionResponseDto(
            record.PlayerId,
            record.PlayerName,
            JwtSecurityTokenHandler.WriteToken(token),
            expiresAtUtc);
    }
}
