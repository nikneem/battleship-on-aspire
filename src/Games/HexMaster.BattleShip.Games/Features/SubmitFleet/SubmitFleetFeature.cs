using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games.Features.SubmitFleet;

public sealed record SubmitFleetCommand(string GameCode, string PlayerId, IReadOnlyList<GameShipPlacement> Ships)
    : ICommand<GameStateResponseDto>;

public sealed class SubmitFleetHandler(IGameRepository gameRepository)
    : ICommandHandler<SubmitFleetCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        SubmitFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        game.SubmitFleet(command.PlayerId, command.Ships);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
