// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Subscription.Base;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Providers.Subscription.Base
{
    public abstract class SubscriptionProvider<TCommand, TResult> : ISubscriptionProvider
        where TCommand : SubscriptionRequest where TResult : ICommandResponse, new()
    {
        protected Session Session;
        protected readonly ILogger Logger;
        protected readonly TCommand Command;
        protected readonly ConnectorConfiguration Settings;
        protected IComplexTypeSystem ComplexTypeSystem;

        protected SubscriptionProvider(TCommand command, IOptions<ConnectorConfiguration> connectorConfiguration, ILogger<SubscriptionProvider<TCommand, TResult>> logger)
        {
            this.Command = command;
            this.Settings = connectorConfiguration.Value;
            this.Logger = logger;
        }

        public async Task<ICommandResponse> ExecuteAsync(Session session, IComplexTypeSystem complexTypeSystem)
        {
            this.Logger.Trace($"Executing {typeof(TCommand).Name}");
            TResult result = default;
            try
            {
                this.Session = session;
                this.ComplexTypeSystem = complexTypeSystem;

                string message;
                try
                {
                    message = await this.ExecuteCommandAsync();
                }
                catch (Exception ex)
                {
                    var error = ex.Demystify();
                    message = $"Failed to execute subscription request: {error.Message}";
                }

                result = ConstructResult();
                this.GenerateResult(result, message);

                return result;
            }
            catch (Exception e)
            {
                this.Logger.Error(e);
            }

            return result;
        }

        protected string EndpointUrl => this.Session.GetBaseEndpointUrl();

        protected Opc.Ua.Client.Subscription GetSubscription(SubscriptionMonitoredItem monitoredItem)
        {
            if (monitoredItem == default) return null;

            var subscriptions = this.Session.Subscriptions
                .Where(x => x.MonitoredItems.Any(y => monitoredItem.NodeId.Equals(y.ResolvedNodeId.ToString())));

            return subscriptions.FirstOrDefault();
        }

        protected abstract Task<string> ExecuteCommandAsync();

        protected virtual void GenerateResult(TResult result, string message) { }

        private static TResult ConstructResult()
        {
            return (TResult)Activator.CreateInstance(typeof(TResult));
        }

        protected string GetStatusMessage(List<string> errorMessages)
        {
            return errorMessages.Any()
                ? string.Join(" ", errorMessages)
                : "Good";
        }
    }
}