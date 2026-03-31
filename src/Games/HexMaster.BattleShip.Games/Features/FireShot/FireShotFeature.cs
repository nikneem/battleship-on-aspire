using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.IntegrationEvents;
using IntegrationShotOutcome = HexMaster.BattleShip.IntegrationEvents.ShotOutcome;

namespace HexMaster.BattleShip.Games.Features.FireShot;

public sealed record FireShotCommand(string GameCode, string PlayerId, GameCoordinate Target)
    : ICommand<GameStateResponseDto>;

public sealed class FireShotHandler(
    IGameRepository gameRepository,
    IEventBus eventBus) : ICommandHandler<FireShotCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        FireShotCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");

        var outcome = game.FireShot(command.PlayerId, command.Target);

        await gameRepository.SaveAsync(game, cancellationToken);
        await eventBus.PublishAsync(
            new ShotFiredIntegrationEvent(
                game.GameCode,
                command.PlayerId,
                command.Target.Row,
                command.Target.Column,
                (IntegrationShotOutcome)(int)outcome),
            cancellationToken);

        if (game.Phase == GamePhase.Finished)
        {
            await eventBus.PublishAsync(
                new GameFinishedIntegrationEvent(game.GameCode, game.WinnerPlayerId!),
                cancellationToken);
        }

        return GameMappings.ToStateResponseDto(game, command.PlayerId);
    }
}
