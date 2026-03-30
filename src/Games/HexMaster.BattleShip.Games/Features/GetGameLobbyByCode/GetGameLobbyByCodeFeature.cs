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
        var game = await gameRepository.GetByCodeAsync(query.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        return GameMappings.ToLobbyResponseDto(game);
    }
}
