// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Application.Factories
{
    public interface IAlarmSubscriptionProviderFactory
    {
        IAlarmSubscriptionProvider GetProvider(ICommandRequest command, TelemetryMessageMetadata telemetryMessageMetadata);
    }
}
