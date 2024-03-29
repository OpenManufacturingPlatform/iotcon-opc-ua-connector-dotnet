﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading;
using Confluent.Kafka;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers
{
    public interface ICustomKafkaConsumer<TKey, TValue>
    {
        IConsumer<TKey, TValue> Consumer { get; }
        ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default);
        ConsumeResult<TKey, TValue> Consume(TimeSpan timeout);
    }
}