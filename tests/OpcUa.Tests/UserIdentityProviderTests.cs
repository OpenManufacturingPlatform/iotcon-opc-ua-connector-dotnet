using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Infrastructure.OpcUa;
using Opc.Ua;
using Xunit;

namespace OpcUa.Tests
{
    public class UserIdentityProviderTests
    {
        // sut (subject under test)
        private IUserIdentityProvider _sut;

        public UserIdentityProviderTests()
        {
            _sut = new UserIdentityProvider(Options.Create(GetConnectorConfig()));
        }

        [Fact]
        public void GetUserIdentity_ShouldReturnAnAnonymousUserIdentity_WhenNoUsernameAndPasswordAreConfigured()
        {
            // Arrange
            var endpointDescription = new EndpointDescription
            {
                EndpointUrl = "opc.tcp://localhost:48020/UA/SampleServer",
                SecurityMode = MessageSecurityMode.None
            };

            // Act
            var userIdentity = _sut.GetUserIdentity(endpointDescription);

            // Assert
            Assert.Equal(UserTokenType.Anonymous, userIdentity.TokenType);
        }

        [Fact]
        public void GetUserIdentity_ShouldReturnUserIdentity_WithUsernameAndPassword()
        {
            // Arrange
            _sut = new UserIdentityProvider(Options.Create(GetConnectorConfig("test", "test")));

            var endpointDescription = new EndpointDescription
            {
                EndpointUrl = "opc.tcp://localhost:48020/UA/SampleServer",
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                Server = new ApplicationDescription
                {
                    DiscoveryUrls = new StringCollection
                    {
                        "opc.tcp://localhost:48020/"
                    }
                }
            };

            // Act
            var userIdentity = _sut.GetUserIdentity(endpointDescription);

            // Assert
            Assert.Equal(UserTokenType.UserName, userIdentity.TokenType);
        }

        #region PrivateMethods
        private ConnectorConfiguration GetConnectorConfig(string username = "", string password = "")
        {
            return new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration
                {
                    Authentication = new List<AuthenticationConfiguration>
                    {
                        new AuthenticationConfiguration
                        {
                            Endpoint = "opc.tcp://localhost:48020/UA/sampleServer/plantMunich",
                            Username = username,
                            Password = password
                        }
                    }
                }
            };
        }
        #endregion

    }
}
