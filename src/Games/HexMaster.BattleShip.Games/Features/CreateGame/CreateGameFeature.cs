using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;

namespace HexMaster.BattleShip.Games.Features.CreateGame;

public sealed record CreateGameCommand(string HostPlayerId, string HostPlayerName, string? JoinSecret)
    : ICommand<CreateGameResponseDto>;

public sealed class CreateGameHandler(
    IGameRepository gameRepository,
    IGameCodeGenerator gameCodeGenerator,
    IGameSecretHasher secretHasher) : ICommandHandler<CreateGameCommand, CreateGameResponseDto>
{
    public async Task<CreateGameResponseDto> HandleAsync(
        CreateGameCommand command,
        CancellationToken cancellationToken = default)
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
        return GameMappings.ToCreateGameResponseDto(game);
    }
}
