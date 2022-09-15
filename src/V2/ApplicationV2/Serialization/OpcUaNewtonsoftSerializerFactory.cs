// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Serialization
{
    public sealed class OpcUaNewtonsoftSerializerFactory : IOmpOpcUaSerializerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public OpcUaNewtonsoftSerializerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IOmpOpcUaSerializer Create(IOpcUaSession opcUaSession, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true)
        {
            return new OpcUaNewtonsoftSerializer(opcUaSession, useReversibleEncoding, useGenericEncoderOnError, loggerFactory.CreateLogger<OpcUaNewtonsoftSerializer>());
        }
    }
}
