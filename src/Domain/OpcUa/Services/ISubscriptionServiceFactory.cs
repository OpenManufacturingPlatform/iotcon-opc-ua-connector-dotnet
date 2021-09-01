namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionServiceFactory
    {
        ISubscriptionService Create();
    }
}