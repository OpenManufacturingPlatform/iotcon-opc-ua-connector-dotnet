// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

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
        SentConfigToBroker = 20
    }
}