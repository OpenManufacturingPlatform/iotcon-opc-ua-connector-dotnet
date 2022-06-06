// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Tests.Fakes;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers.Commands;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;

namespace OMP.Connector.Application.Tests.Services
{
    [TestFixture]
    public class CommandServiceTests
    {
        [Test]
        public Task ExecuteAsync_Causes_Exception_Returns_Error()
            => this.Test_When_Raising_An_Exception<Exception>();

        [Test]
        public Task ExecuteAsync_Causes_ServiceResultException_Returns_Error()
            => this.Test_When_Raising_An_Exception<ServiceResultException>("Failed to execute command request on Unit Test Endpoint, ErrorMessage: A UA specific error occurred.");

        [Test]
        public async Task ExecuteAsync_For_Unknown_Command_Succeed_With_An_Error_Message()
        {
            // Arrange
            var commandService = CommandServiceSetup.CreateCommandService(TestConstants.SchemaUrl);
            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command);

            // Test
            var actual = await commandService.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteAsync_With_Null_OpcUaSession_Must_Succeed_With_An_Error_Message()
        {
            // Arrange
            var fakeSession = FakeOpcUaSession.Create<FakeOpcUaSessionWithCustomRead>();
            var readResponse = fakeSession.ReturnNullReferenceReadResponse(TestConstants.NodeId);
            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command, commandResponses: readResponse);

            var commandProvider = Substitute.For<ICommandProvider>();

            commandProvider
                .ExecuteAsync()
                .Returns(new[] { readResponse });

            var commandProcessorFactory = Substitute.For<ICommandProviderFactory>();
            commandProcessorFactory
                .GetProcessors(Arg.Any<IEnumerable<ICommandRequest>>(), Arg.Any<IOpcSession>())
                .Returns(new[] { commandProvider });

            var commandService = CommandServiceSetup.CreateCommandService(TestConstants.SchemaUrl, commandProcessorFactory: commandProcessorFactory);

            // Test
            var actual = await commandService.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteAsync_ReadCommand_Must_Succeed()
        {
            // Arrange
            var fakeSession = FakeOpcUaSession.Create<FakeOpcUaSessionWithCustomRead>();
            var readResponse = fakeSession.ReturnGoodReadResponse(TestConstants.NodeId);
            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command, commandResponses: readResponse);


            var commandProvider = Substitute.For<ICommandProvider>();

            commandProvider
                .ExecuteAsync()
                .Returns(new[] { readResponse });

            var commandProcessorFactory = Substitute.For<ICommandProviderFactory>();
            commandProcessorFactory
                .GetProcessors(Arg.Any<IEnumerable<ICommandRequest>>(), Arg.Any<IOpcSession>())
                .Returns(new[] { commandProvider });

            var commandService = CommandServiceSetup.CreateCommandService(TestConstants.SchemaUrl, null, commandProcessorFactory);

            // Test
            var actual = await commandService.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteAsync_With_Null_ServerDetails_Returns_Error()
        {
            // Arrange
            var commandProvider = Substitute.For<ICommandProvider>();
            var fakeSession = FakeOpcUaSession.Create<FakeOpcUaSessionWithCustomRead>();
            var readResponse = fakeSession.ReturnBadReadResponse(TestConstants.NodeId, "Failed to execute command request on Unit Test Endpoint");
            ICommandResponse[] response = { readResponse };
            commandProvider
                .ExecuteAsync()
                .Returns(response);

            var commandProcessorFactory = Substitute.For<ICommandProviderFactory>();
            commandProcessorFactory
                .GetProcessors(Arg.Any<IEnumerable<ICommandRequest>>(), Arg.Any<IOpcSession>())
                .Returns(new[] { commandProvider });

            var commandService = CommandServiceSetup.CreateCommandService(TestConstants.SchemaUrl, commandProcessorFactory: commandProcessorFactory, setupServerDetailsInEndpointRepo: false);

            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command, commandResponses: response);

            var actual = await commandService.ExecuteAsync(command);

            CommandHelper.AreEqual(expected, actual);
        }

        private async Task Test_When_Raising_An_Exception<T>(string errorMessage = null)
            where T : Exception, new()
        {
            // Arrange
            var commandProcessorFactory = Substitute.For<ICommandProviderFactory>();
            commandProcessorFactory
                .GetProcessors(Arg.Any<IEnumerable<ICommandRequest>>(), Arg.Any<IOpcSession>())
                .Throws(new T());

            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandResponseCreator.GetErrorResponseMessage(TestConstants.SchemaUrl, command);
            var commandService = CommandServiceSetup.CreateCommandService(TestConstants.SchemaUrl, commandProcessorFactory: commandProcessorFactory);

            // Test
            var actual = await commandService.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }
    }
}