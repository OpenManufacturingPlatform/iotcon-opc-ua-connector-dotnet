// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Models;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;

namespace OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint
{
    public interface IConfigurationConsumer : ICustomKafkaConsumer<string, AppConfigDto>
    { }
}