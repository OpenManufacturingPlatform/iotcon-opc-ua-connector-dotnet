// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IOpcAlarmMonitoredItemService : IDisposable
    {
        public void Initialize(AlarmSubscriptionMonitoredItem monitoredItemCommand, TelemetryMessageMetadata messageMetadata, IOpcSession session);

        public TelemetryMessageMetadata MessageMetadata { get; set; }
    }
}
