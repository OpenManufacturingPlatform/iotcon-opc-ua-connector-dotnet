using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Responses.Control;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Tests.Support;

namespace OMP.Connector.Application.Tests.Providers
{
    [TestFixture]
    public class ReadProviderTests
    {
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(10, 13)]
        [TestCase(50, 30000)]
        public async Task Should_Perform_Correct_Number_Of_Batched_Reads(int batchSize, int totalItems)
        {
            // Arrange
            var readCommands = this.GenerateDummyData(totalItems).ToList();
            var expectedBatches = BatchSupport.CalculateBatches(batchSize, totalItems);
            var expectedReadResults = GetExpectedReadResults(readCommands);

            var opcSession = SetupOpcSessionForRead.CreateOpcSession();
            var readProvider = ReadProviderSetup.CreateReadProvider(opcSession, readCommands, batchSize);

            // Test
            var responses = await readProvider.ExecuteAsync();

            // Verify
            expectedReadResults.Should().BeEquivalentTo(responses.ToList(), options => options.RespectingRuntimeTypes());
            opcSession.Session.ReceivedWithAnyArgs(expectedBatches.Count).Read(default, default, default, default, out _, out _);
        }

        [Test]
        public void Should_Return_Null_Response_When_No_Nodes_Provided()
        {
            // Arrange
            var numberOfNodes = 0;
            var batchSize = 10;
            var readCommands = this.GenerateDummyData(numberOfNodes).ToList();
            var expectedBatches = BatchSupport.CalculateBatches(batchSize, numberOfNodes);

            var opcSession = SetupOpcSessionForRead.CreateOpcSession();
            var readProvider = ReadProviderSetup.CreateReadProvider(opcSession, readCommands, batchSize);

            // Test
            var responses = readProvider.ExecuteAsync().GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual(default, responses);
            opcSession.Session.ReceivedWithAnyArgs(expectedBatches.Count).Read(default, default, default, default, out _, out _);
        }

        private static List<ICommandResponse> GetExpectedReadResults(List<string> nodesToRead)
        {
            var expectedValues = new List<ICommandResponse>();
            foreach (var nodeToRead in nodesToRead)
            {
                //node value simulated by taking number in nodeid
                var nodeId = nodeToRead.ToString();
                var indexOfNumber = nodeId.LastIndexOf("=") + 1;
                var number = nodeId[indexOfNumber..];
                expectedValues.Add(new ReadResponse()
                { 
                    DataType=TestConstants.ExpectedNodeDataType,
                    Message= TestConstants.ExpectedCommandGoodMessage,
                    NodeId=nodeId,
                    OpcUaCommandType=OpcUaCommandType.Read, Value=new SensorMeasurementString(number)
                });
            };
            return expectedValues;
        }

        private List<string> GenerateDummyData(int size)
        {
            var list = new List<string>();
            for (int i = 1; i <= size; i++)
            {
                list.Add($"ns=2;i={i}");
            }
            return list;
        }
    }
}