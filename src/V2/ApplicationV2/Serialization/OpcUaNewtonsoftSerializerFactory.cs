// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Serialization
{
    public sealed class OpcUaNewtonsoftSerializerFactory : IOmpOpcUaSerializerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public OpcUaNewtonsoftSerializerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IOmpOpcUaSerializer Create(IOpcUaSession opcUaSession, bool useReversibleEncoding = false, bool useGenericEncoderOnError = true)
        {
            return new OpcUaNewtonsoftSerializer(opcUaSession, useReversibleEncoding, useGenericEncoderOnError, loggerFactory.CreateLogger<OpcUaNewtonsoftSerializer>());
        }
    }
}
