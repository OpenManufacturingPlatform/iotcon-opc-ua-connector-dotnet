namespace OMP.Connector.Domain.Enums
{
    public enum EventTypes
    {
        ReceivedRequestFromBroker = 3,
        QueuedMessageInternally = 4,
        DequeuedMessageInternally = 5,
        SentRequestToOpcUa = 6,
        ReceivedResponseFromOpcUa = 7,
        SentResponseToBroker = 8,
        SentTelemetryToBroker = 11,
        SentAlarmToBroker = 12,
        SentConfigToBroker = 20
    }
}