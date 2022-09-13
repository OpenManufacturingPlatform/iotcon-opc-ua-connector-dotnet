// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Serialization
{
    public sealed class OpcUaNewtonSofSerializerFactory : IOmpOpcUaSerializerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public OpcUaNewtonSofSerializerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IOmpOpcUaSerializer Create(IOpcUaSession opcUaSession, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true)
        {
            return new OpcUaNewtonSofSerializer(opcUaSession, useReversibleEncoding, useGenericEncoderOnError, loggerFactory.CreateLogger<OpcUaNewtonSofSerializer>());
        }
    }
}
