// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using OMP.Connector.Domain.Models;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{
    public interface IKafkaApplicationConfigurationRepository
    {
        void Initialize(AppConfigDto applicationConfig);
    }
}