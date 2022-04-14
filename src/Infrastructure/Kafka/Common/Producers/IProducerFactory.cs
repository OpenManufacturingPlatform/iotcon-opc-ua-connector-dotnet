﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint;
using OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers
{
    public interface IProducerFactory
    {
        IConfigurationProducer CreateConfigurationProducer();
        IResponseProducer CreateResponseProducer();
        ITelemetryProducer CreateTelemetryProducer();
    }
}
