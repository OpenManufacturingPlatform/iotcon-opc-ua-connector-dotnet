using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.M2Mqtt;
using Xunit;

namespace MQTT.Tests
{
    public class M2MqttClientFactoryTests
    {
        [Fact]
        public void Ensure_CreateClient_Returns_new_Connection_When_Mqtt_Broker_Differs()
        {
            var channelConfiguration = new MqttConfigurationForTests
            {
                Type = CommunicationType.MQTT
            };

            var configuration = Substitute.For<IConfiguration>();

            channelConfiguration.SetNativeConfig(configuration, null);
            var logger = Substitute.For<ILogger<M2MqttClient>>();
            var factory = new M2MqttClientFactory(logger);

            channelConfiguration.MqttClientSettings!.BrokerAddress = "localhost";
            channelConfiguration.MqttClientSettings!.BrokerPort = 8080;

            var client = factory.CreateClient(channelConfiguration, null);

            //change broker address and test that two connection are returned

            channelConfiguration.MqttClientSettings!.BrokerAddress = "127.0.0.1";
            channelConfiguration.MqttClientSettings!.BrokerPort = 8080;

            var client2 = factory.CreateClient(channelConfiguration, null);

            //change broker address and test that two connection are returned

            channelConfiguration.MqttClientSettings!.BrokerAddress = "localhost";
            channelConfiguration.MqttClientSettings!.BrokerPort = 8081;

            var client3 = factory.CreateClient(channelConfiguration, null);

            client.Should().NotBeEquivalentTo(client2);
            client.Should().NotBeEquivalentTo(client3);
            client2.Should().NotBeEquivalentTo(client3);
        }


        [Fact]
        public void Ensure_CreateClient_Fails_With_Invalid_Broker_Address()
        {
            var channelConfiguration = new MqttConfigurationForTests
            {
                Type = CommunicationType.MQTT
            };

            var configuration = Substitute.For<IConfiguration>();

            channelConfiguration.SetNativeConfig(configuration, null);
            var logger = Substitute.For<ILogger<M2MqttClient>>();
            var factory = new M2MqttClientFactory(logger);
            
            channelConfiguration.MqttClientSettings!.BrokerAddress = "Some INVALID broker address to force the ctor of the M2MqttClient to break if it is called again";

            Assert.Throws<AggregateException>(() => factory.CreateClient(channelConfiguration, null));
        }

        [Fact]
        public void Ensure_CreateClient_Only_Creates_One_Connection_Per_ClientId()
        {
            var channelConfiguration = new MqttConfigurationForTests
            {
                Type = CommunicationType.MQTT
            };

            var configuration = Substitute.For<IConfiguration>();

            channelConfiguration.SetNativeConfig(configuration, null);
            var logger = Substitute.For<ILogger<M2MqttClient>>();
            var factory = new M2MqttClientFactory(logger);

            var client = factory.CreateClient(channelConfiguration, null);

            client.Should().NotBeNull();

            channelConfiguration.MqttClientSettings!.BrokerAddress = "Some INVALID broker address to force the ctor of the M2MqttClient to break if it is called again";

            var client2 = factory.CreateClient(channelConfiguration, null);

            client2.Should().NotBeNull();

            client.Should().Be(client2);
        }

        [Fact]
        public void Ensure_CreateClient_Creates_Two_Connections()
        {
            var channelConfiguration = new MqttConfigurationForTests
            {
                Type = CommunicationType.MQTT
            };

            var configuration = Substitute.For<IConfiguration>();

            channelConfiguration.SetNativeConfig(configuration, null);
            var logger = Substitute.For<ILogger<M2MqttClient>>();
            var factory = new M2MqttClientFactory(logger);

            var client = factory.CreateClient(channelConfiguration, null);

            client.Should().NotBeNull();

            channelConfiguration.MqttClientSettings!.ClientId = Guid.NewGuid().ToString();

            var client2 = factory.CreateClient(channelConfiguration, null);

            client2.Should().NotBeNull();

            client.Should().NotBe(client2);
        }
    }
}