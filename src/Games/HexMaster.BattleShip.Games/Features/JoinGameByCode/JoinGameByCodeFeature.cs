using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Abstractions.Services;
using HexMaster.BattleShip.Games.DomainModels;

namespace HexMaster.BattleShip.Games.Features.JoinGameByCode;

public sealed record JoinGameByCodeCommand(
    string GameCode,
    string PlayerId,
    string PlayerName,
    string? JoinSecret) : ICommand<GameLobbyResponseDto>;

public sealed class JoinGameByCodeHandler(
    IGameRepository gameRepository,
    IGameSecretHasher secretHasher) : ICommandHandler<JoinGameByCodeCommand, GameLobbyResponseDto>
{
    public async Task<GameLobbyResponseDto> HandleAsync(
        JoinGameByCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByCodeAsync(command.GameCode, cancellationToken)
                   ?? throw new KeyNotFoundException("The requested game could not be found.");
        var storedSecretHash = game is Game concreteGame ? concreteGame.ToDocument().ProtectedSecretHash ?? string.Empty : string.Empty;
        var secretValidated = !game.IsProtected || secretHasher.VerifySecret(command.JoinSecret ?? string.Empty, storedSecretHash);

        game.JoinGuest(command.PlayerId, command.PlayerName, secretValidated);

        await gameRepository.SaveAsync(game, cancellationToken);
        return GameMappings.ToLobbyResponseDto(game);
    }
}
