using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.LockFleet;

public sealed record LockFleetCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class LockFleetHandler(IGameRepository gameRepository)
    : ICommandHandler<LockFleetCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        LockFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.LockFleet(command.PlayerId);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
