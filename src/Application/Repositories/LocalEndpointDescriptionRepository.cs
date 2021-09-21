using System.Collections.Concurrent;
using System.Collections.Generic;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;

namespace OMP.Connector.Application.Repositories
{
    public class LocalEndpointDescriptionRepository : IEndpointDescriptionRepository
    {
        private readonly IDictionary<string, EndpointDescriptionDto> _endpointDescriptions;

        public LocalEndpointDescriptionRepository()
        {
            this._endpointDescriptions = new ConcurrentDictionary<string, EndpointDescriptionDto>();
        }

        public bool Add(EndpointDescriptionDto description)
        {
            if (this._endpointDescriptions.TryGetValue(description.EndpointUrl, out var cachedEndpoint))
            {
                if (!cachedEndpoint.Equals(description))
                    cachedEndpoint.ServerDetails = description.ServerDetails;
            }
            else
                this._endpointDescriptions.Add(description.EndpointUrl, description);

            return true;
        }

        public EndpointDescriptionDto GetByEndpointUrl(string endpointUrl)
        {
            this._endpointDescriptions.TryGetValue(endpointUrl, out var result);
            return result;
        }
    }
}