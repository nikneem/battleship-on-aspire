using System.Diagnostics;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.CreateGame;

public sealed record CreateGameCommand(string HostPlayerId, string HostPlayerName, string? JoinSecret)
    : ICommand<CreateGameResponseDto>;

public sealed class CreateGameHandler(
    IGameRepository gameRepository,
    IGameCodeGenerator gameCodeGenerator,
    IGameSecretHasher secretHasher,
    IEventBus eventBus) : ICommandHandler<CreateGameCommand, CreateGameResponseDto>
{
    public async Task<CreateGameResponseDto> HandleAsync(
        CreateGameCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = GamesTelemetry.Source.StartActivity("CreateGame");
        activity?.SetTag("game.host_player_id", command.HostPlayerId);
        activity?.SetTag("game.host_player_name", command.HostPlayerName);
        activity?.SetTag("game.is_protected", command.JoinSecret is not null);

        try
        {
            var protectedSecretHash = string.IsNullOrWhiteSpace(command.JoinSecret)
                ? null
                : secretHasher.HashSecret(command.JoinSecret);
            Game? game = null;

            for (var attempt = 0; attempt < 10 && game is null; attempt += 1)
            {
                var gameCode = gameCodeGenerator.GenerateCode();
                var existingGame = await gameRepository.GetByCodeAsync(gameCode, cancellationToken);

                if (existingGame is not null)
                {
                    continue;
                }

                game = Game.Create(command.HostPlayerId, command.HostPlayerName, gameCode, protectedSecretHash);
            }

            if (game is null)
            {
                throw new InvalidOperationException("Failed to generate a unique game code.");
            }

            await gameRepository.SaveAsync(game, cancellationToken);
            await eventBus.PublishAsync(
                new GameCreatedIntegrationEvent(game.GameCode, game.Host.PlayerId, game.Host.PlayerName),
                cancellationToken);

            var result = GameMappings.ToCreateGameResponseDto(game);

            activity?.SetTag("game.code", result.GameCode);
            activity?.SetStatus(ActivityStatusCode.Ok);
            GamesTelemetry.GamesCreated.Add(1);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
