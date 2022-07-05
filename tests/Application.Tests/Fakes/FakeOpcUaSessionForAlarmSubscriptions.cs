// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Security.Cryptography.X509Certificates;
using OMP.Connector.Domain.Schema.Responses.AlarmSubscription;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;

namespace OMP.Connector.Application.Tests.Fakes
{
    public class FakeOpcUaSessionForAlarmSubscriptions : FakeOpcUaSession
    {
        private const string Bad = "Bad";
        private const string Good = "Good";

        public FakeOpcUaSessionForAlarmSubscriptions(
            ITransportChannel channel,
            ApplicationConfiguration appConfig,
            ConfiguredEndpoint endpointConfig,
            X509Certificate2 cert)
            : base(channel, appConfig, endpointConfig, cert)
        { }

        public CreateAlarmSubscriptionsResponse ReturnCreateAlarmSubscriptionsResponse(string message, string nodeId)
        {
            return new CreateAlarmSubscriptionsResponse
            {
                MonitoredItems = new[] {
                    new CreateAlarmSubscriptionItemResponse()
                    {
                        NodeId = nodeId,
                        Message = message
                    }
                },
                Message = message,
                OpcUaCommandType = Domain.Schema.Enums.OpcUaCommandType.CreateSubscription
            };
        }

        public CreateAlarmSubscriptionsResponse ReturnGoodCreateAlarmSubscriptionsResponse(string nodeId)
            => this.ReturnCreateAlarmSubscriptionsResponse(Good, nodeId);
    }
}
