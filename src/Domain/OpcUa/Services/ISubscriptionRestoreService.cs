using System.Threading.Tasks;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface ISubscriptionRestoreService
    {
        Task RestoreSubscriptionsAsync(IOpcSession opcSession);
    }
}