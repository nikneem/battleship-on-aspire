using HexMaster.BattleShip.Core;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Profiles.Abstractions.Validation;
using HexMaster.BattleShip.Profiles.Features.CreateAnonymousPlayerSession;
using HexMaster.BattleShip.Profiles.Features.RenewAnonymousPlayerSession;

namespace HexMaster.BattleShip.Profiles.Endpoints;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profiles").WithTags("Profiles");

        group.MapPost(
                "/anonymous-sessions",
                async (
                    CreateAnonymousPlayerSessionRequestDto requestDto,
                    ICommandHandler<CreateAnonymousPlayerSessionCommand, AnonymousPlayerSessionResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var responseDto = await commandHandler.HandleAsync(
                            new CreateAnonymousPlayerSessionCommand(requestDto.PlayerName),
                            cancellationToken);

                        return Results.Created(
                            $"/api/profiles/anonymous-sessions/{responseDto.PlayerId}",
                            responseDto);
                    }
                    catch (ArgumentException ex) when (ex.ParamName == "playerName")
                    {
                        return Results.ValidationProblem(
                            new Dictionary<string, string[]>
                            {
                                ["playerName"] =
                                [
                                    ex.Message
                                ]
                            });
                    }
                })
            .WithName("CreateAnonymousPlayerSession")
            .WithOpenApi();

        group.MapPost(
                "/anonymous-sessions/renew",
                async (
                    RenewAnonymousPlayerSessionRequestDto requestDto,
                    ICommandHandler<RenewAnonymousPlayerSessionCommand, AnonymousPlayerSessionResponseDto> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var responseDto = await commandHandler.HandleAsync(
                            new RenewAnonymousPlayerSessionCommand(requestDto.AccessToken),
                            cancellationToken);
                        return Results.Ok(responseDto);
                    }
                    catch (AnonymousPlayerSessionRenewalException ex)
                        when (ex.Reason == AnonymousPlayerSessionRenewalFailureReason.TooEarly)
                    {
                        return Results.ValidationProblem(
                            new Dictionary<string, string[]>
                            {
                                ["accessToken"] =
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
}
