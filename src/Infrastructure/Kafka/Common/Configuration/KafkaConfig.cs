// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OMP.Connector.Infrastructure.Kafka.Common.Configuration
{
    public class KafkaConfig
    {
        private IEnumerable<string> _topics;
        public string Topic { get; set; }
        public int MaxRetryAttempts { get; set; } = 5;
        public TimeSpan PauseBetweenFailures { get; set; } = TimeSpan.FromSeconds(5);
        public IEnumerable<string> Topics
        {
            get
            {
                if (_topics != null && _topics.Any())
                    return _topics;

                if (string.IsNullOrWhiteSpace(Topic))
                {
                    _topics = new string[0];
                    return _topics;
                }

                _topics = Topic.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                return _topics;
            }
        }

        public string Separator { get; set; } = ";";
    }
}
