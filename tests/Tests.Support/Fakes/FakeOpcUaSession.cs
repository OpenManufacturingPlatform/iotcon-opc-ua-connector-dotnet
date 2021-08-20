using System.Security.Cryptography.X509Certificates;
using NSubstitute;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Tests.Support.Fakes
{
    public class FakeOpcUaSession : Session
    {
        public FakeOpcUaSession(
            ITransportChannel channel,
            ApplicationConfiguration appConfig,
            ConfiguredEndpoint endpointConfig,
            X509Certificate2 cert)
            : base(channel, appConfig, endpointConfig, cert)
        { }


        public static T Create<T>()
            where T : FakeOpcUaSession
        {
            var channelMock = Substitute.For<ITransportChannel>();
            channelMock
                .EndpointDescription
                .Returns(new EndpointDescription($"http//{nameof(FakeOpcUaSession)}/endpoint"));

            var appConfig = new ApplicationConfiguration()
            {
                ClientConfiguration = new ClientConfiguration(),
                SecurityConfiguration = new SecurityConfiguration(),
                CertificateValidator = new CertificateValidator()
            };
            var endpointConfig = new ConfiguredEndpoint
            {
                Description =
                {
                    SecurityPolicyUri = SecurityPolicies.None
                }
            };

            return Substitute.For<T>(channelMock, appConfig, endpointConfig, null);
        }
    }
}