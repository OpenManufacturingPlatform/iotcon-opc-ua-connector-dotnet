﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;

namespace OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint
{
    public interface IResponseProducer : ICustomKafkaProducer<string, CommandResponse> { }
}