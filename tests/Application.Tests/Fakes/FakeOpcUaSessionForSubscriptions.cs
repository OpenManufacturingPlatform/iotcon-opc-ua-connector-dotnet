// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Security.Cryptography.X509Certificates;
using OMP.Connector.Domain.Schema.Responses.Subscription;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;

namespace OMP.Connector.Application.Tests.Fakes
{
    public class FakeOpcUaSessionForSubscriptions : FakeOpcUaSession
    {
        private const string Bad = "Bad";
        private const string Good = "Good";

        public FakeOpcUaSessionForSubscriptions(
            ITransportChannel channel,
            ApplicationConfiguration appConfig,
            ConfiguredEndpoint endpointConfig,
            X509Certificate2 cert)
            : base(channel, appConfig, endpointConfig, cert)
        { }

        public CreateSubscriptionsResponse ReturnCreateSubscriptionsResponse(string message, string nodeId)
        {
            return new CreateSubscriptionsResponse
            {
                MonitoredItems = new[] {
                    new CreateSubscriptionItemResponse()
                    {
                        NodeId = nodeId,
                        Message = message
                    }
                },
                Message = message,
                OpcUaCommandType = Domain.Schema.Enums.OpcUaCommandType.CreateSubscription
            };
        }

        public CreateSubscriptionsResponse ReturnGoodCreateSubscriptionsResponse(string nodeId)
            => this.ReturnCreateSubscriptionsResponse(Good, nodeId);
    }
}
