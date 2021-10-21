namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IAlarmSubscriptionServiceFactory
    {
        IAlarmSubscriptionService Create();
    }
}