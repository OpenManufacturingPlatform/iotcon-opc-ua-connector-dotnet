// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OMP.PlantConnectivity.OpcUA.Sessions.Subscriptions
{
    internal class FakeSubscriptionRestoreService : ISubscriptionRestoreService
    {
        public Task RestoreSubscriptionsAsync(IOpcUaSession opcUaSession)
        {
            throw new NotImplementedException();
        }
    }

    //public interface ISubscriptionProviderFactory
    //{
    //    ISubscriptionProvider GetProvider(ICommandRequest command, TelemetryMessageMetadata telemetryMessageMetadata);
    //}

    //public class SubscriptionRestoreService : ISubscriptionRestoreService
    //{
    //    private readonly ILogger _logger;
    //    private readonly ISubscriptionProviderFactory _subscriptionProviderFactory;
    //    private readonly ConnectorConfiguration _connectorConfiguration;
    //    private readonly ISubscriptionRepository _subscriptionRepository;
    //    private string _schemaUrl => _connectorConfiguration.Communication.SchemaUrl;

    //    public SubscriptionRestoreService(
    //        ISubscriptionProviderFactory subscriptionProviderFactory,
    //        ISubscriptionRepository subscriptionRepository,
    //        IOptions<ConnectorConfiguration> connectorConfiguration,
    //        ILogger<SubscriptionRestoreService> logger)
    //    {
    //        _logger = logger;
    //        _subscriptionRepository = subscriptionRepository;
    //        _connectorConfiguration = connectorConfiguration.Value;
    //        _subscriptionProviderFactory = subscriptionProviderFactory;
    //    }

    //    private async Task ExecuteSubscriptionRequestAsync(IOpcUaSession opcUaSession, CommandRequest requestMessage)
    //    {
    //        try
    //        {
    //            foreach (var request in requestMessage.Payload.Requests)
    //            {
    //                var provider = _subscriptionProviderFactory.GetProvider(request, TelemetryMessageMetadata.MapFrom(requestMessage));
    //                if (provider == default)
    //                    throw new ApplicationException("Subscription restore command is not supported");

    //                await provider.ExecuteAsync(opcSession);
    //            }
    //        }
    //        catch (Exception exception)
    //        {
    //            _logger.Error(exception, "Subscription restore command failed with error");
    //        }
    //    }

    //    public async Task RestoreSubscriptionsAsync(IOpcSession opcSession)
    //    {
    //        if (_connectorConfiguration.DisableSubscriptionRestoreService)
    //            return;

    //        _logger.Debug("Restoring subscriptions...");

    //        SubscriptionDto[] monitoredItems = default;
    //        try
    //        {
    //            monitoredItems = GetSubscriptions();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.Error(ex, "Could not retrieve subscriptions.");
    //        }

    //        var restoredCount = 0;
    //        if (monitoredItems != default && monitoredItems.Any())
    //        {
    //            try
    //            {
    //                await RestoreSubscriptionsAsync(opcSession, monitoredItems);
    //                restoredCount = monitoredItems.Length;
    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.Error(ex, "Could not restore subscriptions");
    //            }
    //        }

    //        _logger.Information($"Restored [{restoredCount}] subscriptions");
    //    }

    //    private SubscriptionDto[] GetSubscriptions()
    //    {
    //        var subscriptions = _subscriptionRepository.GetAllSubscriptions();
    //        return subscriptions.ToArray();
    //    }

    //    private async Task RestoreSubscriptionsAsync(IOpcSession opcSession, IEnumerable<SubscriptionDto> subscriptions)
    //    {
    //        foreach (var currentSubscription in subscriptions)
    //        {
    //            var request = ConstructCommandRequest(currentSubscription, _schemaUrl);
    //            await this.ExecuteSubscriptionRequestAsync(opcSession, request);
    //        }
    //    }

    //    private static CommandRequest ConstructCommandRequest(SubscriptionDto subscription, string schemaUrl)
    //    {
    //        var request = new CreateSubscriptionsRequest
    //        {
    //            OpcUaCommandType = OpcUaCommandType.CreateSubscription,
    //            MonitoredItems = subscription.MonitoredItems?.Values.ToArray()
    //        };

    //        var commandRequest = ModelFactory.CreateInstance<CommandRequest>(schemaUrl);
    //        commandRequest.Payload = new RequestPayload
    //        {
    //            RequestTarget = new RequestTarget { EndpointUrl = subscription.EndpointUrl },
    //            Requests = new List<ICommandRequest> { request }
    //        };

    //        return commandRequest;
    //    }
    //}
}
