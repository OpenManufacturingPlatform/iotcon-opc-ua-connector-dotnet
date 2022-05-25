// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
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
                return endpointDescription.Server.DiscoveryUrls.Any(url => url == GetDiscoveryUrl(s.Endpoint));
            });

            return setting is null ? new UserIdentity() : new UserIdentity(setting.Username, setting.Password);
        }

        private string GetDiscoveryUrl(string endpoint)
        {
            var uri = new Uri(endpoint);
            var discoveryUrl = $"{uri.Scheme}://{uri.Authority}/";
            return discoveryUrl;
        }
    }
}
