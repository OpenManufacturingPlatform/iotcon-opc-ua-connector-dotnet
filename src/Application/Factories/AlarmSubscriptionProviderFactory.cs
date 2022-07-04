// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.AlarmSubscription;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;

namespace OMP.Connector.Application.Factories
{
    public class AlarmSubscriptionProviderFactory : IAlarmSubscriptionProviderFactory
    {
        private readonly IAlarmSubscriptionRepository _subscriptionRepository;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<ConnectorConfiguration> _connectorConfiguration;
        private readonly IOpcAlarmMonitoredItemService _alarmMonitoredItemService;
        private readonly AbstractValidator<AlarmSubscriptionMonitoredItem> _alarmMonitoredItemValidator;

        public AlarmSubscriptionProviderFactory(
            IAlarmSubscriptionRepository dataManagementService,
            ILoggerFactory loggerFactory,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcAlarmMonitoredItemService alarmMonitoredItemService,
            AbstractValidator<AlarmSubscriptionMonitoredItem> alarmMonitoredItemValidator)
        {
            this._subscriptionRepository = dataManagementService;
            this._loggerFactory = loggerFactory;
            this._connectorConfiguration = connectorConfiguration;
            this._alarmMonitoredItemService = alarmMonitoredItemService;
            this._alarmMonitoredItemValidator = alarmMonitoredItemValidator;
        }

        public IAlarmSubscriptionProvider GetProvider(ICommandRequest command, TelemetryMessageMetadata telemetryMessageMetadata)
            => command switch
            {
                { } when command is CreateAlarmSubscriptionsRequest createCommand => this.CreateAlarmSubscriptionProvider(createCommand, telemetryMessageMetadata),
                { } when command is RemoveAllAlarmSubscriptionsRequest deleteCommand => this.CreateRemoveAllAlarmSubscriptionProvider(deleteCommand),
                { } when command is RemoveAlarmSubscriptionsRequest removeItemsCommand => this.CreateRemoveAlarmSubscriptionProvider(removeItemsCommand),
                { } when command is RespondToAlarmEventsRequest respondToAlarmEventsCommand => this.RespondToAlarmEventsProvider(respondToAlarmEventsCommand),
                _ => default
            };

        private IAlarmSubscriptionProvider CreateAlarmSubscriptionProvider(CreateAlarmSubscriptionsRequest createCommand, TelemetryMessageMetadata telemetryMessageMetadata)
            => new CreateAlarmSubscriptionProvider(
                                this._subscriptionRepository,
                                this._loggerFactory.CreateLogger<CreateAlarmSubscriptionProvider>(),
                                this._connectorConfiguration,
                                this._alarmMonitoredItemService,
                                createCommand,
                                telemetryMessageMetadata,
                                this._alarmMonitoredItemValidator);

        private IAlarmSubscriptionProvider RespondToAlarmEventsProvider(RespondToAlarmEventsRequest respondToAlarmEventsCommand)
            => new RespondToAlarmEventsProvider(
                                this._loggerFactory.CreateLogger<RespondToAlarmEventsProvider>(),
                                this._connectorConfiguration,
                                respondToAlarmEventsCommand);

        private IAlarmSubscriptionProvider CreateRemoveAllAlarmSubscriptionProvider(RemoveAllAlarmSubscriptionsRequest command)
            => new RemoveAllAlarmSubscriptionProvider(
                command,
                this._subscriptionRepository,
                this._connectorConfiguration,
                this._loggerFactory.CreateLogger<RemoveAllAlarmSubscriptionProvider>());

        private IAlarmSubscriptionProvider CreateRemoveAlarmSubscriptionProvider(RemoveAlarmSubscriptionsRequest command)
            => new RemoveAlarmSubscriptionProvider(
                command,
                this._subscriptionRepository,
                this._connectorConfiguration,
                this._loggerFactory.CreateLogger<RemoveAlarmSubscriptionProvider>());
    }
}
