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
using OMP.Connector.Domain.Schema.Alarms;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Services
{
    public class OpcAlarmMonitoredItemService : MonitoredItem, IOpcAlarmMonitoredItemService
    {
        private bool _disposedValue;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMessageSender _messageSender;
        private readonly ConnectorConfiguration _connectorConfiguration;
        private IComplexTypeSystem _complexTypeSystem;
        private OpcUaMonitoredItem _monitoredItemCommand;

        public TelemetryMessageMetadata MessageMetadata { get; set; }

        public OpcAlarmMonitoredItemService(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMessageSender messageSender,
            IMapper mapper,
            ILogger<OpcAlarmMonitoredItemService> logger)
        {
            this._logger = logger;
            this._mapper = mapper;
            this._connectorConfiguration = connectorConfiguration.Value;
            this._messageSender = messageSender;

            this.Notification += this.OnNotification;
        }

        public void Initialize(AlarmSubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata, Session session)
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

            var filter = new FilterDefinition();
            filter.AreaId = monitoredItemCommand.NodeId;
            filter.Severity = EventSeverity.Min;
            filter.IgnoreSuppressedOrShelved = true;
            filter.EventTypes = new NodeId[] { ObjectTypeIds.ConditionType };

            // must specify the fields that the form is interested in.
            filter.SelectClauses = filter.ConstructSelectClauses(
                session,
                ObjectTypeIds.DialogConditionType,
                ObjectTypeIds.ExclusiveLimitAlarmType,
                ObjectTypeIds.NonExclusiveLimitAlarmType);

            // create a monitored item based on the current filter settings.
            filter.UpdateMonitoredItem(this, session);
        }

        private void OnNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                var session = monitoredItem?.Subscription?.Session;
                var msgContent = this.CollectMessageContent(this._monitoredItemCommand, e);

                if (!this.IsValidNotification(msgContent, session))
                    return;

                var alarmSource = this.GetAlarmSource(session);

                var alarmMessage = AlarmUtilities.CreateMessage(msgContent, alarmSource, monitoredItem.Filter, this._mapper);

                this._messageSender.SendMessageToAlarmsAsync(alarmMessage).GetAwaiter().GetResult();

                this._logger.Debug($"Alarm Monitored Item: [{monitoredItem?.StartNodeId}] notification triggered with EventFields: [{msgContent.EventFields.EventFields.Count}]");
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

        private AlarmMessageContent CollectMessageContent(
            OpcUaMonitoredItem monitoredItemCommand,
            MonitoredItemNotificationEventArgs eventArgs)
        {
            var msgContent = new AlarmMessageContent
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

            if (eventArgs?.NotificationValue is EventFieldList eventFieldList)
            {
                msgContent.EventFields = eventFieldList;
            }
            return msgContent;
        }

        private AlarmSource GetAlarmSource(Session session)
        {
            var endpointUrl = session.GetBaseEndpointUrl();

            return new AlarmSource
            {
                Id = string.Empty,
                Name = session.Endpoint?.Server?.ApplicationName?.Text ?? string.Empty,
                Route = session.Endpoint?.EndpointUrl ?? string.Empty,
                EndpointUrl = endpointUrl
            };
        }

        private bool IsValidNotification(AlarmMessageContent alarmMessageContent, Session session)
        {
            return alarmMessageContent.EventFields.EventFields.Count > 0 && session != default;
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
