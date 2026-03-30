using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.MarkReady;

public sealed record MarkReadyCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class MarkReadyHandler(IGameRepository gameRepository)
    : ICommandHandler<MarkReadyCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        MarkReadyCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.MarkReady(command.PlayerId);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
