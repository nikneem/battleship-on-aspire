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
        var game = await gameRepository.GetByCodeAsync(query.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        return GameMappings.ToStateResponseDto(game, query.PlayerId);
    }
}
