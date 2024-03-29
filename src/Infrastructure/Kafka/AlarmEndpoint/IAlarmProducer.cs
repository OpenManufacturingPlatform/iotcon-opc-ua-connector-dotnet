﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;

namespace OMP.Connector.Infrastructure.Kafka.AlarmEndpoint
{
    public interface IAlarmProducer : ICustomKafkaProducer<string, AlarmMessage> { }
}
