namespace HexMaster.BattleShip.IntegrationEvents;

public sealed record PlayerConnectionLostIntegrationEvent(
    string GameCode,
    string PlayerId,
    DateTimeOffset DisconnectedAt) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record PlayerConnectionReestablishedIntegrationEvent(
    string GameCode,
    string PlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record PlayerConnectionTimedOutIntegrationEvent(
    string GameCode,
    string PlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}
