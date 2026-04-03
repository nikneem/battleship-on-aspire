using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.LockFleet;

public sealed record LockFleetCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class LockFleetHandler(
    IGameRepository gameRepository,
    IEventBus eventBus,
    IRandomProvider randomProvider) : ICommandHandler<LockFleetCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        LockFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("LockFleet");
        activity?.SetTag("game.code", command.GameCode);
        activity?.SetTag("game.player_id", command.PlayerId);

        try
        {
            await using var _ = await gameRepository.BeginUpdateAsync(command.GameCode, cancellationToken);

            var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            var hostGoesFirst = randomProvider.NextBool();
            game.LockFleet(command.PlayerId, hostGoesFirst);

            await gameRepository.SaveAsync(game, cancellationToken);
            await eventBus.PublishAsync(
                new FleetLockedIntegrationEvent(game.GameCode, command.PlayerId),
                cancellationToken);

            if (game.Phase == GamePhase.InProgress)
            {
                await eventBus.PublishAsync(
                    new GameStartedIntegrationEvent(game.GameCode, game.CurrentTurnPlayerId!),
                    cancellationToken);
            }

            var result = GameMappings.ToStateResponseDto(game, command.PlayerId);

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
