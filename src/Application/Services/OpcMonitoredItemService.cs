// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using Opc.Ua;
using Opc.Ua.Client;
using TelemetryUtilities = OMP.Connector.Application.OpcUa.TelemetryUtilities;

namespace OMP.Connector.Application.Services
{
    public class OpcMonitoredItemService : MonitoredItem, IOpcMonitoredItemService
    {
        private bool _disposedValue;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMessageSender _messageSender;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private IComplexTypeSystem _complexTypeSystem;
        private OpcUaMonitoredItem _monitoredItemCommand;

        public TelemetryMessageMetadata MessageMetadata { get; set; }

        public OpcMonitoredItemService(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMessageSender messageSender,
            IMapper mapper,
            ILogger<OpcMonitoredItemService> logger
            )
        {
            this._logger = logger;
            this._mapper = mapper;
            this._connectorConfiguration = connectorConfiguration.Value;
            this._messageSender = messageSender;

            this.Notification += this.OnNotification;
        }

        public void Initialize(SubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata)
        {
            this.MessageMetadata = messageMetadata;
            this.StartNodeId = monitoredItemCommand.NodeId;
            this.AttributeId = Attributes.Value;
            this.MonitoringMode = MonitoringMode.Reporting;
            this.SamplingInterval = int.Parse(monitoredItemCommand.SamplingInterval);
            this.QueueSize = 1;
            this.DiscardOldest = false;

            this._complexTypeSystem = complexTypeSystem;
            this._monitoredItemCommand = monitoredItemCommand;
        }

        private void OnNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                var session = monitoredItem?.Subscription?.Session;
                var msgContent = this.CollectMessageContent(this._monitoredItemCommand, e);

                if (!this.IsValidNotification(msgContent, session))
                    return;

                this.AddElementTypeWhenAvailableAsync(monitoredItem?.StartNodeId, msgContent, session).GetAwaiter().GetResult();

                var telemetrySource = this.GetSensorTelemetrySource(session);

                var telemetryMessage = TelemetryUtilities.CreateMessage(msgContent, telemetrySource, this._mapper);

                this._messageSender.SendMessageToTelemetryAsync(telemetryMessage).GetAwaiter().GetResult();

                this._logger.Debug($"Monitored Item: [{monitoredItem?.StartNodeId}] notification triggered with value: [{msgContent.DataValue.Value}]");
            }
            catch (Exception ex)
            {
                this._logger.Error(ex);
            }
        }

        private async Task AddElementTypeWhenAvailableAsync(NodeId nodeId, TelemetryMessageContent msgContent, Session session)
        {
            var typeInfo = msgContent.DataDataType;
            if (TelemetryConversion.IsArray(typeInfo.ValueRank) && typeInfo.BuiltInType == BuiltInType.ExtensionObject)
            {
                var node = session.NodeCache.Find(nodeId);
                if (node is VariableNode variableNode)
                {
                    var nodeType = await this._complexTypeSystem.LoadType(variableNode.DataType);
                    msgContent.ElementType = nodeType.Name;
                }
            }
        }

        private TelemetryMessageContent CollectMessageContent(
            OpcUaMonitoredItem monitoredItemCommand,
            MonitoredItemNotificationEventArgs eventArgs)
        {
            var msgContent = new TelemetryMessageContent
            {
                SenderId = this.MessageMetadata.Sender?.Id ?? string.Empty,
                SenderName = this.MessageMetadata.Sender?.Name ?? string.Empty,
                SenderRoute = this.MessageMetadata.Sender?.Route ?? string.Empty,
                DataSourceId = string.Empty,
                DataSourceName = string.Empty,
                DataSourceRoute = string.Empty,
                DataSourceUrl = this.MessageMetadata.DataSourceUrl,
                DataKey = monitoredItemCommand.NodeId,
                DataDescription = monitoredItemCommand.NodeId,
                SchemaUrl = this._connectorConfiguration.Communication.SchemaUrl
            };

            if (eventArgs?.NotificationValue is MonitoredItemNotification notification)
            {
                msgContent.SequenceNr = notification.Message.SequenceNumber.ToString();
                msgContent.DataValue = notification.Value;
                msgContent.DataDataType = notification.Value.WrappedValue.TypeInfo;
            }
            return msgContent;
        }

        private SensorTelemetrySource GetSensorTelemetrySource(Session session)
        {
            var endpointUrl = session.GetBaseEndpointUrl();

            return new SensorTelemetrySource
            {
                Id = string.Empty,
                Name = session.Endpoint?.Server?.ApplicationName?.Text ?? string.Empty,
                Route = session.Endpoint?.EndpointUrl ?? string.Empty,
                EndpointUrl = endpointUrl
            };
        }

        private bool IsValidNotification(TelemetryMessageContent telemetryMessageContent, Session session)
        {
            return telemetryMessageContent.DataValue.Value != default && session != default;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposedValue)
                return;

            if (disposing)
            {
                this.Notification -= this.OnNotification;
            }

            this._monitoredItemCommand = null;
            this._disposedValue = true;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
