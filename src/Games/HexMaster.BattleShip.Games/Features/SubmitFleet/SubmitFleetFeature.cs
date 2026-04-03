using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Models;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.SubmitFleet;

public sealed record SubmitFleetCommand(string GameCode, string PlayerId, IReadOnlyList<GameShipPlacement> Ships)
    : ICommand<GameStateResponseDto>;

public sealed class SubmitFleetHandler(
    IGameRepository gameRepository,
    IEventBus eventBus) : ICommandHandler<SubmitFleetCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        SubmitFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("SubmitFleet");
        activity?.SetTag("game.code", command.GameCode);
        activity?.SetTag("game.player_id", command.PlayerId);
        activity?.SetTag("game.fleet.ship_count", command.Ships.Count);

        try
        {
            await using var _ = await gameRepository.BeginUpdateAsync(command.GameCode, cancellationToken);

            var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            game.SubmitFleet(command.PlayerId, command.Ships);

            await gameRepository.SaveAsync(game, cancellationToken);
            await eventBus.PublishAsync(
                new FleetSubmittedIntegrationEvent(game.GameCode, command.PlayerId),
                cancellationToken);

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
