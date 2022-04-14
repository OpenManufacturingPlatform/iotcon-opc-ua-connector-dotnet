﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace OMP.Connector.Infrastructure.Kafka.Serialization
{
    public class KafkaJsonSerializer<TPayload> : IDeserializer<TPayload>, ISerializer<TPayload>
    {
        public TPayload Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var payload = Deserializers.Utf8.Deserialize(data, isNull, context);
            if (payload is null)
                return default;

            return JsonConvert.DeserializeObject<TPayload>(
                payload,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Include,
                    DateParseHandling = DateParseHandling.None
                });
        }

        public byte[] Serialize(TPayload data, SerializationContext context)
            => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.None));
    }
}