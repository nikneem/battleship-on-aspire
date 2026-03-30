using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.CancelGame;

public sealed record CancelGameCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class CancelGameHandler(IGameRepository gameRepository)
    : ICommandHandler<CancelGameCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        CancelGameCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.Cancel(command.PlayerId);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
