// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Linq;
using System.Security.Cryptography.X509Certificates;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;
using ReadResponse = OMP.Connector.Domain.Schema.Responses.Control.ReadResponse;

namespace OMP.Connector.Application.Tests.Fakes
{
    public class FakeOpcUaSessionWithCustomRead : FakeOpcUaSession
    {
        private const string Bad = "Bad";
        private const string Good = "Good";
        private string readResponseTemplate;

        public FakeOpcUaSessionWithCustomRead(
            ITransportChannel channel,
            ApplicationConfiguration appConfig,
            ConfiguredEndpoint endpointConfig,
            X509Certificate2 cert)
            : base(channel, appConfig, endpointConfig, cert)
        { }

        public override ResponseHeader Read(RequestHeader requestHeader, double maxAge, TimestampsToReturn timestampsToReturn,
            ReadValueIdCollection nodesToRead, out DataValueCollection results, out DiagnosticInfoCollection diagnosticInfos)
        {
            var responseHeader = new ResponseHeader();
            results = new DataValueCollection();
            results.AddRange(nodesToRead.Select(t => new DataValue { Value = $"{this.readResponseTemplate} | Node:{t.NodeId.Identifier}" }));
            diagnosticInfos = new DiagnosticInfoCollection();
            return responseHeader;
        }

        public ReadResponse ReturnNullReferenceReadResponse(string nodeId = TestConstants.NodeId)
            => this.ReturnBadReadResponse(nodeId, "Object reference not set to an instance of an object.");

        public ReadResponse ReturnBadReadResponse(string nodeId, string value, string dataType = null)
            => this.ReturnReadResponse(Bad, nodeId, value, dataType);

        public ReadResponse ReturnReadResponse(string message, string nodeId, string value, string dataType = null)
        {
            this.readResponseTemplate = value;
            return new ReadResponse
            {
                DataType = dataType ?? string.Empty,
                Message = message,
                NodeId = nodeId,
                Value = new SensorMeasurementString($"{value} | Node:{nodeId}")
            };
        }

        public ReadResponse ReturnGoodReadResponse(string nodeId, string value = "Read Response for Node", string dataType = "String")
            => this.ReturnReadResponse(Good, nodeId, value, dataType);
    }
}