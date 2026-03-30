using HexMaster.BattleShip.Core;
using HexMaster.BattleShip.Profiles.Abstractions.DataTransferObjects;

namespace HexMaster.BattleShip.Profiles.Features.CreateAnonymousPlayerSession;

public sealed record CreateAnonymousPlayerSessionCommand(string PlayerName)
    : ICommand<AnonymousPlayerSessionResponseDto>;
