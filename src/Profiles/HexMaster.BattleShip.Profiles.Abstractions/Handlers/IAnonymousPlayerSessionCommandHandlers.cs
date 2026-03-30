using HexMaster.BattleShip.Profiles.Abstractions.Commands;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;

namespace HexMaster.BattleShip.Profiles.Abstractions.Handlers;

public interface ICreateAnonymousPlayerSessionCommandHandler
{
    Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        CreateAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default);
}

public interface IRenewAnonymousPlayerSessionCommandHandler
{
    Task<AnonymousPlayerSessionResponseDto> HandleAsync(
        RenewAnonymousPlayerSessionCommand command,
        CancellationToken cancellationToken = default);
}
