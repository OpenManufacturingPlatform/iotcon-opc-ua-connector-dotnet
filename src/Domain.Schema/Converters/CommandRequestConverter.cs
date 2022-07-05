// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Schema.Converters.Base;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Extenions;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription;
using OMP.Connector.Domain.Schema.Request.Base;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Domain.Schema.Request.Discovery;
using OMP.Connector.Domain.Schema.Request.Subscription;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class CommandRequestConverter : CustomJsonConverter<ICommandRequest>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override ICommandRequest Create(System.Type objectType, JToken jToken)
        {
            var propertyName = typeof(CommandRequest).GetPropertyName(nameof(CommandRequest.OpcUaCommandType));
            var dataProperty = this.GetPropertyValue<string>(jToken, propertyName);
            var commandType = dataProperty.ToEnum<OpcUaCommandType>();

            return commandType switch
            {
                OpcUaCommandType.Read => new ReadRequest(),
                OpcUaCommandType.Write => new WriteRequest(),
                OpcUaCommandType.Call => new CallRequest(),
                OpcUaCommandType.Browse => new BrowseRequest(),
                OpcUaCommandType.CreateSubscription => new CreateSubscriptionsRequest(),
                OpcUaCommandType.RemoveAllSubscriptions => new RemoveAllSubscriptionsRequest(),
                OpcUaCommandType.RemoveSubscriptions => new RemoveSubscriptionsRequest(),
                OpcUaCommandType.RestoreSubscriptions => new RestoreSubscriptionsRequest(),
                OpcUaCommandType.CreateAlarmSubscription => new CreateAlarmSubscriptionsRequest(),
                OpcUaCommandType.RespondToAlarmEvents => new RespondToAlarmEventsRequest(),
                OpcUaCommandType.RemoveAllAlarmSubscriptions => new RemoveAllAlarmSubscriptionsRequest(),
                OpcUaCommandType.RemoveAlarmSubscriptions => new RemoveAlarmSubscriptionsRequest(),
                OpcUaCommandType.RestoreAlarmSubscriptions => new RestoreAlarmSubscriptionsRequest(),
                OpcUaCommandType.ServerDiscovery => new ServerDiscoveryRequest(),
                OpcUaCommandType.BrowseChildNodes => new BrowseChildNodesRequest(),
                OpcUaCommandType.BrowseChildNodesFromRoot => new BrowseChildNodesFromRootRequest(),
                _ => default
            };
        }
    }
}