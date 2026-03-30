using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.AbandonGame;

public sealed record AbandonGameCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class AbandonGameHandler(IGameRepository gameRepository)
    : ICommandHandler<AbandonGameCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        AbandonGameCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.Abandon(command.PlayerId);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
