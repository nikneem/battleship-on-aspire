using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.CancelGame;

public sealed record CancelGameCommand(string GameCode, string PlayerId)
    : ICommand<GameStateResponseDto>;

public sealed class CancelGameHandler(
    IGameRepository gameRepository,
    IEventBus eventBus) : ICommandHandler<CancelGameCommand, GameStateResponseDto>
{
    public async Task<GameStateResponseDto> HandleAsync(
        CancelGameCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("CancelGame");
        activity?.SetTag("game.code", command.GameCode);
        activity?.SetTag("game.player_id", command.PlayerId);

        try
        {
            await using var _ = await gameRepository.BeginUpdateAsync(command.GameCode, cancellationToken);

            var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                       ?? throw new KeyNotFoundException("The requested game could not be found.");

            game.Cancel(command.PlayerId);

            await gameRepository.SaveAsync(game, cancellationToken);
            await eventBus.PublishAsync(
                new GameCancelledIntegrationEvent(game.GameCode, command.PlayerId),
                cancellationToken);

            var result = GameMappings.ToStateResponseDto(game, command.PlayerId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            GamesTelemetry.GamesCancelled.Add(1);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
