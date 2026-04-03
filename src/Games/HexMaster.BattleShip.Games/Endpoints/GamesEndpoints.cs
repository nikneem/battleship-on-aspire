using System.Security.Claims;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Features.AbandonGame;
using HexMaster.BattleShip.Games.Features.CancelGame;
using HexMaster.BattleShip.Games.Features.CreateGame;
using HexMaster.BattleShip.Games.Features.FireShot;
using HexMaster.BattleShip.Games.Features.GetGameLobbyByCode;
using HexMaster.BattleShip.Games.Features.GetGameStateForPlayer;
using HexMaster.BattleShip.Games.Features.JoinGameByCode;
using HexMaster.BattleShip.Games.Features.LockFleet;
using HexMaster.BattleShip.Games.Features.MarkReady;
using HexMaster.BattleShip.Games.Features.SubmitFleet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace HexMaster.BattleShip.Games.Endpoints;

public static class GamesEndpoints
{
    private const string PlayerIdClaimType = "sub";
    private const string PlayerNameClaimType = "player_name";

    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games").WithTags("Games").RequireAuthorization();

        group.MapPost(
                "/",
                async (
                    CreateGameRequestDto requestDto,
                    ClaimsPrincipal user,
                    ICommandHandler<CreateGameCommand, CreateGameResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () =>
                        {
                            var response = await commandHandler.HandleAsync(
                                new CreateGameCommand(player.PlayerId, player.PlayerName, requestDto.JoinSecret),
                                cancellationToken);

                            return Results.Created($"/api/games/{response.GameCode}", response);
                        });
                })
            .WithName("CreateGame");

        group.MapPost(
                "/join",
                async (
                    JoinGameByCodeRequestDto requestDto,
                    ClaimsPrincipal user,
                    ICommandHandler<JoinGameByCodeCommand, GameLobbyResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new JoinGameByCodeCommand(
                                requestDto.GameCode,
                                player.PlayerId,
                                player.PlayerName,
                                requestDto.JoinSecret),
                            cancellationToken)));
                })
            .WithName("JoinGameByCode");

        group.MapGet(
                "/{gameCode}",
                async (
                    string gameCode,
                    ClaimsPrincipal user,
                    IQueryHandler<GetGameStateForPlayerQuery, GameStateResponseDto> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await queryHandler.HandleAsync(
                            new GetGameStateForPlayerQuery(gameCode, player.PlayerId),
                            cancellationToken)));
                })
            .WithName("GetGameStateForPlayer");

        group.MapGet(
                "/{gameCode}/lobby",
                async (
                    string gameCode,
                    IQueryHandler<GetGameLobbyByCodeQuery, GameLobbyResponseDto> queryHandler,
                    CancellationToken cancellationToken) =>
                    await Execute(
                        async () => Results.Ok(await queryHandler.HandleAsync(
                            new GetGameLobbyByCodeQuery(gameCode),
                            cancellationToken))))
            .WithName("GetGameLobbyByCode");

        group.MapPost(
                "/{gameCode}/ready",
                async (
                    string gameCode,
                    ClaimsPrincipal user,
                    ICommandHandler<MarkReadyCommand, GameStateResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new MarkReadyCommand(gameCode, player.PlayerId),
                            cancellationToken)));
                })
            .WithName("MarkPlayerReady");

        group.MapPut(
                "/{gameCode}/fleet",
                async (
                    string gameCode,
                    SubmitFleetRequestDto requestDto,
                    ClaimsPrincipal user,
                    ICommandHandler<SubmitFleetCommand, GameStateResponseDto> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken cancellationToken) =>
                {
                    var logger = loggerFactory.CreateLogger("GamesEndpoints.SubmitFleet");
                    logger.LogInformation("SubmitFleet: gameCode={GameCode}, ships={ShipCount}",
                        gameCode, requestDto.Ships?.Count.ToString() ?? "NULL");

                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new SubmitFleetCommand(gameCode, player.PlayerId, GameMappings.ToShipPlacements(requestDto.Ships)),
                            cancellationToken)),
                        loggerFactory.CreateLogger("GamesEndpoints"));
                })
            .WithName("SubmitFleet");

        group.MapPost(
                "/{gameCode}/lock",
                async (
                    string gameCode,
                    ClaimsPrincipal user,
                    ICommandHandler<LockFleetCommand, GameStateResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new LockFleetCommand(gameCode, player.PlayerId),
                            cancellationToken)));
                })
            .WithName("LockFleet");

        group.MapPost(
                "/{gameCode}/shots",
                async (
                    string gameCode,
                    FireShotRequestDto requestDto,
                    ClaimsPrincipal user,
                    ICommandHandler<FireShotCommand, GameStateResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new FireShotCommand(gameCode, player.PlayerId, GameMappings.ToCoordinate(requestDto.Target)),
                            cancellationToken)));
                })
            .WithName("FireShot");

        group.MapPost(
                "/{gameCode}/cancel",
                async (
                    string gameCode,
                    CancelGameRequestDto _,
                    ClaimsPrincipal user,
                    ICommandHandler<CancelGameCommand, GameStateResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new CancelGameCommand(gameCode, player.PlayerId),
                            cancellationToken)));
                })
            .WithName("CancelGame");

        group.MapPost(
                "/{gameCode}/abandon",
                async (
                    string gameCode,
                    AbandonGameRequestDto _,
                    ClaimsPrincipal user,
                    ICommandHandler<AbandonGameCommand, GameStateResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    if (!TryReadPlayer(user, out var player))
                    {
                        return Results.Unauthorized();
                    }

                    return await Execute(
                        async () => Results.Ok(await commandHandler.HandleAsync(
                            new AbandonGameCommand(gameCode, player.PlayerId),
                            cancellationToken)));
                })
            .WithName("AbandonGame");

        return endpoints;
    }

    private static async Task<IResult> Execute(Func<Task<IResult>> callback, ILogger? logger = null)
    {
        try
        {
            return await callback();
        }
        catch (ArgumentException ex)
        {
            logger?.LogWarning(ex, "Fleet validation failed (ArgumentException): {Message}", ex.Message);
            return Results.ValidationProblem(ToValidationErrors(ex));
        }
        catch (InvalidOperationException ex)
        {
            logger?.LogWarning(ex, "Fleet validation failed (InvalidOperationException): {Message}", ex.Message);
            return Results.ValidationProblem(ToValidationErrors(ex));
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }

    private static bool TryReadPlayer(ClaimsPrincipal user, out AuthenticatedPlayer player)
    {
        player = default!;

        var playerId = user.FindFirst(PlayerIdClaimType)?.Value
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var playerName = user.FindFirst(PlayerNameClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(playerName))
        {
            return false;
        }

        player = new AuthenticatedPlayer(playerId, playerName);
        return true;
    }

    private static Dictionary<string, string[]> ToValidationErrors(Exception exception)
    {
        return new Dictionary<string, string[]>
        {
            [exception is ArgumentException argumentException && !string.IsNullOrWhiteSpace(argumentException.ParamName)
                ? argumentException.ParamName
                : "game"] =
            [
                exception.Message
            ]
        };
    }

    private sealed record AuthenticatedPlayer(string PlayerId, string PlayerName);
}
