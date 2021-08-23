using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Producers.Responses;
using OneOf;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
{
    public interface IConfigurationPersister
    {
        Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> SaveConfigurationAsync(AppConfigDto appConfig, CancellationToken cancellationToken);
    }
}
