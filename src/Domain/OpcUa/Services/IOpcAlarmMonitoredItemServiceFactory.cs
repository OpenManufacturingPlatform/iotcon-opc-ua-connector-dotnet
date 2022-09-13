// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IOpcAlarmMonitoredItemServiceFactory
    {
        IOpcAlarmMonitoredItemService Create(AlarmSubscriptionMonitoredItem alarmMonitoredItemCommand, TelemetryMessageMetadata messageMetadata, IOpcSession opcSession);
    }
}
