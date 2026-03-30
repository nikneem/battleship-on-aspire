using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.FireShot;

public sealed record FireShotCommand(string GameCode, string PlayerId, GameCoordinate Target)
    : ICommand<GameStateResponseDto>;

public sealed class FireShotHandler(IGameRepository gameRepository)
    : ICommandHandler<FireShotCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        FireShotCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.FireShot(command.PlayerId, command.Target);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
