using OMP.Connector.Domain;
using Opc.Ua.Client;
using Opc.Ua.Client.ComplexTypes;

namespace OMP.Connector.Infrastructure.OpcUa
{
    public class ComplexTypeSystemWithInterface: ComplexTypeSystem, IComplexTypeSystem
    {
        public ComplexTypeSystemWithInterface(Session session)
            : base(session)
        {}

        public ComplexTypeSystemWithInterface(Session session, IComplexTypeFactory complexTypeBuilderFactory)
            : base(session, complexTypeBuilderFactory)
        {}
    }
}
