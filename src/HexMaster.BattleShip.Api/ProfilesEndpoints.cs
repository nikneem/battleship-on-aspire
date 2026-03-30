using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Commands;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Handlers;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using Microsoft.AspNetCore.Authorization;

namespace HexMaster.BattleShip.Api;

public static class ProfilesEndpoints
{
    private const int MaxPlayerNameLength = 40;

    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profiles").WithTags("Profiles");

        group.MapPost(
                "/anonymous-sessions",
                async (
                    CreateAnonymousPlayerSessionRequestDto requestDto,
                    ICreateAnonymousPlayerSessionCommandHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var validationErrors = ValidateCreateAnonymousSessionRequest(requestDto);

                    if (validationErrors.Count > 0)
                    {
                        return Results.ValidationProblem(validationErrors);
                    }

                    var responseDto = await commandHandler.HandleAsync(
                        new CreateAnonymousPlayerSessionCommand(requestDto.PlayerName),
                        cancellationToken);

                    return Results.Created(
                        $"/api/profiles/anonymous-sessions/{responseDto.PlayerId}",
                        responseDto);
                })
            .WithName("CreateAnonymousPlayerSession")
            .WithOpenApi();

        group.MapPost(
                "/anonymous-sessions/renew",
                [Authorize] async (
                    RenewAnonymousPlayerSessionRequestDto _,
                    ClaimsPrincipal user,
                    IRenewAnonymousPlayerSessionCommandHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryMapRenewCommand(user, out var command))
                    {
                        return Results.Unauthorized();
                    }

                    try
                    {
                        var responseDto = await commandHandler.HandleAsync(command, cancellationToken);
                        return Results.Ok(responseDto);
                    }
                    catch (AnonymousPlayerSessionRenewalException ex)
                        when (ex.Reason == AnonymousPlayerSessionRenewalFailureReason.TooEarly)
                    {
                        return Results.ValidationProblem(
                            new Dictionary<string, string[]>
                            {
                                ["token"] =
                                [
                                    ex.Message
                                ]
                            });
                    }
                    catch (AnonymousPlayerSessionRenewalException)
                    {
                        return Results.Unauthorized();
                    }
                })
            .WithName("RenewAnonymousPlayerSession")
            .WithOpenApi();

        return endpoints;
    }

    private static Dictionary<string, string[]> ValidateCreateAnonymousSessionRequest(
        CreateAnonymousPlayerSessionRequestDto requestDto)
    {
        var validationErrors = new Dictionary<string, string[]>();
        var normalizedPlayerName = requestDto.PlayerName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedPlayerName))
        {
            validationErrors["playerName"] = ["Player name is required."];
        }
        else if (normalizedPlayerName.Length > MaxPlayerNameLength)
        {
            validationErrors["playerName"] = [$"Player name must be {MaxPlayerNameLength} characters or fewer."];
        }

        return validationErrors;
    }

    private static bool TryMapRenewCommand(
        ClaimsPrincipal user,
        out RenewAnonymousPlayerSessionCommand command)
    {
        command = default!;

        var playerId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var playerName = user.FindFirst(AnonymousPlayerClaimNames.PlayerName)?.Value;
        var exp = user.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (string.IsNullOrWhiteSpace(playerId) ||
            string.IsNullOrWhiteSpace(playerName) ||
            !long.TryParse(exp, out var expirationTimeUnixSeconds))
        {
            return false;
        }

        command = new RenewAnonymousPlayerSessionCommand(
            playerId,
            playerName,
            DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnixSeconds));

        return true;
    }
}
