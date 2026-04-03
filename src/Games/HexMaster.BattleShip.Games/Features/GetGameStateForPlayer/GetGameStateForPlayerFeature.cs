using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.GetGameStateForPlayer;

public sealed record GetGameStateForPlayerQuery(string GameCode, string PlayerId)
    : IQuery<GameStateResponseDto>;

public sealed class GetGameStateForPlayerHandler(IGameRepository gameRepository)
    : IQueryHandler<GetGameStateForPlayerQuery, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        GetGameStateForPlayerQuery query,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("GetGameStateForPlayer");
        activity?.SetTag("game.code", query.GameCode);
        activity?.SetTag("game.player_id", query.PlayerId);

        try
        {
            var game = await gameRepository.GetByCodeAsync(query.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            var result = GameMappings.ToStateResponseDto(game, query.PlayerId);

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
