using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Infrastructure.Kafka.Extensions
{
    internal static class MonitoredItemExtensions
    {
        public static bool EqualsByValue(this SubscriptionMonitoredItem monitoredItem1, SubscriptionMonitoredItem monitoredItem2)
        {
            return (monitoredItem1.HeartbeatInterval == monitoredItem2.HeartbeatInterval &&
                    monitoredItem1.PublishingInterval == monitoredItem2.PublishingInterval &&
                    monitoredItem1.SamplingInterval == monitoredItem2.SamplingInterval &&
                    monitoredItem1.NodeId == monitoredItem2.NodeId);
        }
    }
}
