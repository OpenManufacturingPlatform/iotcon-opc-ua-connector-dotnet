// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Application.Tests.TestSetup
{
    public static class TestConstants
    {
        internal const string NodeId = "UnitTestNodeId";
        internal const string SchemaUrl = "https://schema.unit.test/schema.json";
        internal const string ExpectedServerName = "Server name for unit test";
        internal const string ExpectedServerRoute = "Route name for unit test";
        internal const string ExpectedSchemaUrl = "https://schema.unit.test/schema.json/message_schemas/2020-03-31/bmw.iot.models.messages.opcua.commandresponse.schema.json";
        internal const string ExpectedNodeDataType = "String";
        internal const string ExpectedCommandGoodMessage = "Good";
        internal const string ExpectedResponseId = "Response message Id";
        internal const string ExpectedCorrelationId = "Correlation Id";
        internal const string ExpectedEndpointUrl = "opc.tcp://test";
        internal const string ExpectedConnectorId = "Connector Id";
        internal const string CommandBadMessage = "Bad";
    }
}
