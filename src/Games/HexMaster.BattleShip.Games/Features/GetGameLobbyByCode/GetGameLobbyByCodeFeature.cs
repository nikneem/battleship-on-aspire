using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.GetGameLobbyByCode;

public sealed record GetGameLobbyByCodeQuery(string GameCode)
    : IQuery<GameLobbyResponseDto>;

public sealed class GetGameLobbyByCodeHandler(IGameRepository gameRepository)
    : IQueryHandler<GetGameLobbyByCodeQuery, GameLobbyResponseDto>
{
    public async Task<GameLobbyResponseDto> HandleAsync(
        GetGameLobbyByCodeQuery query,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("GetGameLobbyByCode");
        activity?.SetTag("game.code", query.GameCode);

        try
        {
            var game = await gameRepository.GetByCodeAsync(query.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            var result = GameMappings.ToLobbyResponseDto(game);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
