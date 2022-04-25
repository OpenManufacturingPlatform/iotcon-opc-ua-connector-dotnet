// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading;
using System.Threading.Tasks;
using OMP.Connector.Domain.Models;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses;
using OneOf;

namespace OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint
{
    public interface IConfigurationPersister
    {
        Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> SaveConfigurationAsync(AppConfigDto appConfig, CancellationToken cancellationToken);
    }
}
