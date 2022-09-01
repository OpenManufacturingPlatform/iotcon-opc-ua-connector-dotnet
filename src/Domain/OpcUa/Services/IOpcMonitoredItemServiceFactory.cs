// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IOpcMonitoredItemServiceFactory
    {
        IOpcMonitoredItemService Create(SubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata);
    }
}
