// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Tests.Fakes;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Tests.Services
{
    [TestFixture]
    public class SubscriptionServiceTests
    {
        [Test]
        public async Task ExecuteSubscriptionRequestAsync_With_Null_OpcUaSession_Must_Succeed_With_An_Error_Message()
        {
            // Arrange
            IOpcSession nullOpcUaSession = null;
            var sessionPoolStateManager = Substitute.For<ISessionPoolStateManager>();
            sessionPoolStateManager
                .GetSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(nullOpcUaSession);

            var service =
                SubscriptionServiceSetup.CreateSubscriptionService(TestConstants.SchemaUrl, sessionPoolStateManager);
            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var errorMessage =
                $"Failed to execute subscription request on {command.Payload.RequestTarget.EndpointUrl}, ErrorMessage: Object reference not set to an instance of an object.";
            var expected = CommandResponseCreator.GetErrorResponseMessage(TestConstants.SchemaUrl, command);

            // Test
            var actual = await service.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteSubscriptionRequestAsync_For_Unknown_Command_Succeed_With_An_Error_Message()
        {
            // Arrange
            var sessionPoolStateManager = Substitute.For<ISessionPoolStateManager>();

            var service = SubscriptionServiceSetup.CreateSubscriptionService(TestConstants.SchemaUrl, sessionPoolStateManager);

            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command);

            // Test
            var actual = await service.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteSubscriptionRequestAsync_Must_Succeed()
        {
            // Arrange
            var fakeSession = FakeOpcUaSession.Create<FakeOpcUaSessionWithCustomRead>();
            var opcSession = Substitute.For<IOpcSession>();
            await opcSession
                .UseAsync(Arg.Invoke<Session, IComplexTypeSystem>(fakeSession, null));

            var readResponse = fakeSession.ReturnGoodReadResponse(TestConstants.NodeId);
            var sessionPoolStateManager = Substitute.For<ISessionPoolStateManager>();
            sessionPoolStateManager
                .GetSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(opcSession);

            var service = SubscriptionServiceSetup.CreateSubscriptionService(TestConstants.SchemaUrl, sessionPoolStateManager, expectedResponse: readResponse);

            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command, commandResponses: readResponse);

            // Test
            var actual = await service.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteSubscriptionRequestAsync_With_ApplicationException__With_An_Error_Message()
        {
            // Arrange
            var sessionPoolStateManager = Substitute.For<ISessionPoolStateManager>();

            var service = SubscriptionServiceSetup.CreateSubscriptionService(TestConstants.SchemaUrl, sessionPoolStateManager, setupSubscriptionProviderFactory: false);

            var command = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandHelper.CreateReadCommandResponse(TestConstants.SchemaUrl, command);

            // Test
            var actual = await service.ExecuteAsync(command);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }

        [Test]
        public Task ExecuteSubscriptionRequestAsync_Causes_Exception_Returns_Error()
            => this.Test_When_Raising_An_Exception<Exception>();


        [Test]

        public Task ExecuteSubscriptionRequestAsync_Causes_ServiceResultException_Returns_Error()
            => this.Test_When_Raising_An_Exception<ServiceResultException>("Failed to execute subscription request on Unit Test Endpoint, ErrorMessage: A UA specific error occurred.");

        private async Task Test_When_Raising_An_Exception<T>(string errorMessage = null)
            where T : Exception, new()
        {
            // Arrange
            var sessionPoolStateManager = Substitute.For<ISessionPoolStateManager>();
            sessionPoolStateManager
                .GetSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(new T());

            var requestMessage = CommandHelper.CreateReadCommandRequest(TestConstants.NodeId, TestConstants.SchemaUrl);
            var expected = CommandResponseCreator.GetErrorResponseMessage(TestConstants.SchemaUrl, requestMessage);
            var service = SubscriptionServiceSetup.CreateSubscriptionService(TestConstants.SchemaUrl, sessionPoolStateManager);

            // Test
            var actual = await service.ExecuteAsync(requestMessage);

            // Verify
            CommandHelper.AreEqual(expected, actual);
        }
    }
}