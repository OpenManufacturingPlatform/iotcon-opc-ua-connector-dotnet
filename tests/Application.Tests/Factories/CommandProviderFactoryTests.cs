using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.Providers.Commands;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Control;

namespace OMP.Connector.Application.Tests.Factories
{
    [TestFixture]
    public class CommandProviderFactoryTests
    {
        [Test]
        public void GetProcessors_Must_Succeed()
        {
            var opcUaSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration
                {
                    EnableRegisteredNodes = false
                }
            });
            var opcSession = Substitute.For<IOpcSession>();
            var mapper = Substitute.For<IMapper>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var factory = new CommandProviderFactory(opcUaSettings, mapper, loggerFactory);
            var commands = new ICommandRequest[]
            {
                new BrowseRequest(),
                new CallRequest(),
                new WriteRequest(),
                new ReadRequest()
            };

            var actual = factory.GetProcessors(commands, opcSession);
            /* Assert */
            actual
                .Should()
                .HaveCount(commands.Length)
                .And
                .NotContain(o => o == null)
                .And
                .Contain(o => o.GetType() == typeof(BrowseProvider))
                .And
                .Contain(o => o.GetType() == typeof(CallProvider))
                .And
                .Contain(o => o.GetType() == typeof(ReadProvider))
                .And
                .Contain(o => o.GetType() == typeof(WriteProvider));
        }

        [Test]
        public void GetProcessors_Must_Return_Registered_Read()
        {
            var opcUaSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration
                {
                    EnableRegisteredNodes = true,
                }
            });

            var opcSession = Substitute.For<IOpcSession>();
            var mapper = Substitute.For<IMapper>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var factory = new CommandProviderFactory(opcUaSettings, mapper, loggerFactory);
            var commands = new ICommandRequest[]
            {
                new ReadRequest()
            };

            var actual = factory.GetProcessors(commands, opcSession);
            /* Assert */
            actual
                .Should()
                .HaveCount(commands.Length)
                .And
                .NotContainNulls()
                .And
                .ContainItemsAssignableTo<RegisteredReadProvider>();
        }

        [Test]
        public void GetProcessors_Must_Return_Registered_Write()
        {
            var opcUaSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaSettings.Value.Returns(_ => new ConnectorConfiguration
            {
               OpcUa = new OpcUaConfiguration
               {
                   EnableRegisteredNodes = true,
               }
            });

            var opcSession = Substitute.For<IOpcSession>();
            var mapper = Substitute.For<IMapper>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var factory = new CommandProviderFactory(opcUaSettings, mapper, loggerFactory);
            var commands = new ICommandRequest[]
            {
                new WriteRequest()
            };

            var actual = factory.GetProcessors(commands, opcSession);
            /* Assert */
            actual
                .Should()
                .HaveCount(commands.Length)
                .And
                .NotContainNulls()
                .And
                .ContainItemsAssignableTo<RegisteredWriteProvider>();
        }
    }
}
