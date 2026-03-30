using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;

namespace HexMaster.BattleShip.Profiles.Features.RenewAnonymousPlayerSession;

public sealed record RenewAnonymousPlayerSessionCommand(
    string AccessToken)
    : ICommand<AnonymousPlayerSessionResponseDto>;
