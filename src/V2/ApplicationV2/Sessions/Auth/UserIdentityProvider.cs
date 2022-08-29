// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Configuration;
using Microsoft.Extensions.Options;
using Opc.Ua;

namespace ApplicationV2.Sessions.Auth
{
    internal class UserIdentityProvider : IUserIdentityProvider
    {
        private readonly List<AuthenticationConfiguration> authenticationSettings;

        public UserIdentityProvider(IOptions<OmpOpcUaConfiguration> opcUaConfiguration)
        {
            authenticationSettings = opcUaConfiguration.Value.Authentication;
        }

        public IUserIdentity GetUserIdentity(EndpointDescription endpointDescription)
        {
            if (endpointDescription.SecurityMode < MessageSecurityMode.SignAndEncrypt)
                return new UserIdentity();

            var setting = authenticationSettings?.Find(s =>
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
