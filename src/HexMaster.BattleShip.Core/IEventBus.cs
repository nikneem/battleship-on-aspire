using HexMaster.BattleShip.IntegrationEvents;

namespace HexMaster.BattleShip.Core.Eventing;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
