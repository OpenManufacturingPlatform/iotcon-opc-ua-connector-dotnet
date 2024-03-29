﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;

namespace OMP.Connector.Application.Providers.AlarmSubscription.Base
{
    public abstract class AlarmSubscriptionProvider<TCommand, TResult> : IAlarmSubscriptionProvider
        where TCommand : AlarmSubscriptionRequest where TResult : ICommandResponse, new()
    {
        protected IOpcSession OpcSession;
        protected readonly ILogger Logger;
        protected readonly TCommand Command;
        protected readonly ConnectorConfiguration Settings;

        protected AlarmSubscriptionProvider(TCommand command, IOptions<ConnectorConfiguration> connectorConfiguration, ILogger<AlarmSubscriptionProvider<TCommand, TResult>> logger)
        {
            this.Command = command;
            this.Settings = connectorConfiguration.Value;
            this.Logger = logger;
        }

        public async Task<ICommandResponse> ExecuteAsync(IOpcSession opcSession)
        {
            this.Logger.Trace($"Executing {typeof(TCommand).Name}");
            TResult result = default;
            try
            {
                this.OpcSession = opcSession;

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

        protected string EndpointUrl => this.OpcSession.Session.GetBaseEndpointUrl();

        protected Opc.Ua.Client.Subscription GetSubscription(AlarmSubscriptionMonitoredItem monitoredItem)
        {
            if (monitoredItem == default) return null;

            var subscriptions = this.OpcSession.Session.Subscriptions
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
                ? string.Join(" | ", errorMessages)
                : "Good";
        }
    }
}
