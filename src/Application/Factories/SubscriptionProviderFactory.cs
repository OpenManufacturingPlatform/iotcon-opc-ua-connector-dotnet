﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Providers.Subscription;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Subscription;

namespace OMP.Connector.Application.Factories
{
    public class SubscriptionProviderFactory : ISubscriptionProviderFactory
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<ConnectorConfiguration> _connectorConfiguration;
        private readonly IOpcMonitoredItemServiceFactory _monitoredItemServiceFactory;
        private readonly AbstractValidator<SubscriptionMonitoredItem> _monitoredItemValidator;

        public SubscriptionProviderFactory(
            ISubscriptionRepository dataManagementService,
            ILoggerFactory loggerFactory,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IOpcMonitoredItemServiceFactory opcMonitoredItemServiceFactory,
            AbstractValidator<SubscriptionMonitoredItem> monitoredItemValidator)
        {
            this._subscriptionRepository = dataManagementService;
            this._loggerFactory = loggerFactory;
            this._connectorConfiguration = connectorConfiguration;
            this._monitoredItemServiceFactory = opcMonitoredItemServiceFactory;
            this._monitoredItemValidator = monitoredItemValidator;
        }

        public ISubscriptionProvider GetProvider(ICommandRequest command, TelemetryMessageMetadata telemetryMessageMetadata)
            => command switch
            {
                { } when command is CreateSubscriptionsRequest createCommand => this.CreateSubscriptionProvider(createCommand, telemetryMessageMetadata),
                { } when command is RemoveAllSubscriptionsRequest deleteCommand => this.CreateRemoveAllSubscriptionProvider(deleteCommand),
                { } when command is RemoveSubscriptionsRequest removeItemsCommand => this.CreateRemoveSubscriptionProvider(removeItemsCommand),
                _ => default
            };

        private ISubscriptionProvider CreateSubscriptionProvider(CreateSubscriptionsRequest createCommand, TelemetryMessageMetadata telemetryMessageMetadata)
            => new CreateSubscriptionProvider(
                                this._subscriptionRepository,
                                this._loggerFactory.CreateLogger<CreateSubscriptionProvider>(),
                                this._connectorConfiguration,
                                this._monitoredItemServiceFactory,
                                createCommand,
                                telemetryMessageMetadata,
                                this._monitoredItemValidator);

        private ISubscriptionProvider CreateRemoveAllSubscriptionProvider(RemoveAllSubscriptionsRequest command)
            => new RemoveAllSubscriptionProvider(
                command,
                this._subscriptionRepository,
                this._connectorConfiguration,
                this._loggerFactory.CreateLogger<RemoveAllSubscriptionProvider>());

        private ISubscriptionProvider CreateRemoveSubscriptionProvider(RemoveSubscriptionsRequest command)
            => new RemoveSubscriptionProvider(
                command,
                this._subscriptionRepository,
                this._connectorConfiguration,
                this._loggerFactory.CreateLogger<RemoveSubscriptionProvider>());
    }
}
