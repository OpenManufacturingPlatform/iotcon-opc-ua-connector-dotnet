using System;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{
    public partial class KafkaRepository : IEndpointDescriptionRepository
    {
        public bool Add(EndpointDescriptionDto description)
        {
            WaitForInitializationToComplete();

            var cacheChanged = false;
            var persisted = false;

            if (_endpointDescriptions.TryGetValue(description.EndpointUrl, out var cachedEndpoint))
            {
                if (!cachedEndpoint.Equals(description))
                {
                    cachedEndpoint.ServerDetails = description.ServerDetails;
                    cacheChanged = true;
                }
            }
            else
            {
                _endpointDescriptions.Add(description.EndpointUrl, description);
                cacheChanged = true;
            }

            if (cacheChanged)
                persisted = PersistCachedConfig();

            return (cacheChanged && persisted) || (!cacheChanged);
            throw new NotImplementedException();
        }

        public EndpointDescriptionDto GetByEndpointUrl(string endpointUrl)
        {
            WaitForInitializationToComplete();

            _endpointDescriptions.TryGetValue(endpointUrl, out var result);

            return result;
        }
    }
}
