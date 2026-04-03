using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.AbandonGame;

public sealed record AbandonGameCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class AbandonGameHandler(
    IGameRepository gameRepository,
    IEventBus eventBus) : ICommandHandler<AbandonGameCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        AbandonGameCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("AbandonGame");
        activity?.SetTag("game.code", command.GameCode);
        activity?.SetTag("game.player_id", command.PlayerId);

        try
        {
            await using var _ = await gameRepository.BeginUpdateAsync(command.GameCode, cancellationToken);

            var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            game.Abandon(command.PlayerId);

            await gameRepository.SaveAsync(game, cancellationToken);
            await eventBus.PublishAsync(
                new GameAbandonedIntegrationEvent(game.GameCode, command.PlayerId),
                cancellationToken);

            var result = GameMappings.ToStateResponseDto(game, command.PlayerId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            GamesTelemetry.GamesAbandoned.Add(1);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
