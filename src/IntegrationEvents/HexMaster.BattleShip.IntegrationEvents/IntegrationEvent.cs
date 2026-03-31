namespace HexMaster.BattleShip.IntegrationEvents;

public abstract record IntegrationEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public abstract string SchemaVersion { get; }
}
