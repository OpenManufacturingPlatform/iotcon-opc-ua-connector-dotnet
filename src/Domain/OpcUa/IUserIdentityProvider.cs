using Opc.Ua;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IUserIdentityProvider
    {
        IUserIdentity GetUserIdentity(EndpointDescription endpointDescription);
    }
}