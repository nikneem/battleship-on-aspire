using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;
using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Games.Features.JoinGameByCode;

public sealed record JoinGameByCodeCommand(
    string GameCode,
    string PlayerId,
    string PlayerName,
    string? JoinSecret) : ICommand<GameLobbyResponseDto>;

public sealed class JoinGameByCodeHandler(
    IGameRepository gameRepository,
    IGameSecretHasher secretHasher,
    IEventBus eventBus) : ICommandHandler<JoinGameByCodeCommand, GameLobbyResponseDto>
{
    public async Task<GameLobbyResponseDto> HandleAsync(
        JoinGameByCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        await using var _ = await gameRepository.BeginUpdateAsync(command.GameCode, cancellationToken);

        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");
        var storedSecretHash = game is Game concreteGame ? concreteGame.ToDocument().ProtectedSecretHash ?? string.Empty : string.Empty;
        var secretValidated = !game.IsProtected || secretHasher.VerifySecret(command.JoinSecret ?? string.Empty, storedSecretHash);

        game.JoinGuest(command.PlayerId, command.PlayerName, secretValidated);

        await gameRepository.SaveAsync(game, cancellationToken);
        await eventBus.PublishAsync(
            new PlayerJoinedGameIntegrationEvent(game.GameCode, command.PlayerId, command.PlayerName),
            cancellationToken);

        return GameMappings.ToLobbyResponseDto(game);
    }
}
