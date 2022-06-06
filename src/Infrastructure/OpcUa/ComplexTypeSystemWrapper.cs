// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain;
using Opc.Ua.Client;
using Opc.Ua.Client.ComplexTypes;

namespace OMP.Connector.Infrastructure.OpcUa
{
    public class ComplexTypeSystemWrapper: ComplexTypeSystem, IComplexTypeSystem
    {
        public ComplexTypeSystemWrapper(Session session)
            : base(session)
        {}

        public ComplexTypeSystemWrapper(Session session, IComplexTypeFactory complexTypeBuilderFactory)
            : base(session, complexTypeBuilderFactory)
        {}
    }
}
