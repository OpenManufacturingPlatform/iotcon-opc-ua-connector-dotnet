// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa
{
    public class UserIdentityProvider : IUserIdentityProvider
    {
        private readonly List<AuthenticationConfiguration> _authenticationSettings;

        public UserIdentityProvider(
            IOptions<ConnectorConfiguration> connectorConfiguration
            )
        {
            _authenticationSettings = connectorConfiguration.Value.OpcUa.Authentication;
        }

        public IUserIdentity GetUserIdentity(EndpointDescription endpointDescription)
        {
            if (endpointDescription.SecurityMode < MessageSecurityMode.SignAndEncrypt)
                return new UserIdentity();

            var setting = _authenticationSettings?.Find(s =>
            {
                var endpointCollection = GetEndpoints(s.Endpoint);
                var endpointDescriptions = endpointCollection.OrderByDescending(e => e.SecurityLevel);

                var endpoint = endpointDescriptions?.FirstOrDefault(x =>
                        x.EndpointUrl.Equals(endpointDescription.EndpointUrl) &&
                        x.SecurityMode.Equals(endpointDescription.SecurityMode));

                return endpoint != null;
            });

            return setting is null ? new UserIdentity() : new UserIdentity(setting.Username, setting.Password);
        }

        private EndpointDescriptionCollection GetEndpoints(string endpoint)
        {
            var uri = new System.Uri(endpoint);
            var client = DiscoveryClient.Create(uri);
            var endpointCollection = client.GetEndpoints(default);
            return endpointCollection;
        }
    }
}
