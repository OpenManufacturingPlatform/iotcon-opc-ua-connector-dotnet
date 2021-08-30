using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Schema.Converters.Base;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Extenions;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Responses.Base;
using OMP.Connector.Domain.Schema.Responses.Control;
using OMP.Connector.Domain.Schema.Responses.Discovery;
using OMP.Connector.Domain.Schema.Responses.Subscription;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class CommandResponseConverter : CustomJsonConverter<ICommandResponse>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override ICommandResponse Create(System.Type objectType, JToken jToken)
        {
            var propertyName = typeof(CommandResponse).GetPropertyName(nameof(CommandResponse.OpcUaCommandType));
            var dataProperty = this.GetPropertyValue<string>(jToken, propertyName);
           var commandType =  dataProperty.ToEnum<OpcUaCommandType>();

           return commandType switch
           {
               OpcUaCommandType.Read => new ReadResponse(),
               OpcUaCommandType.Write => new WriteResponse(),
               OpcUaCommandType.Call => new CallResponse(),
               OpcUaCommandType.Browse => new BrowseResponse(),
               OpcUaCommandType.CreateSubscription => new CreateSubscriptionsResponse(),
               OpcUaCommandType.RemoveSubscriptions => new RemoveSubscriptionsResponse(),
               OpcUaCommandType.RemoveAllSubscriptions => new RemoveAllSubscriptionsResponse(),
               OpcUaCommandType.RestoreSubscriptions => new CreateSubscriptionsResponse(),
               OpcUaCommandType.ServerDiscovery => new ServerDiscoveryResponse(),
               OpcUaCommandType.BrowseChildNodes => new BrowseChildNodesResponse(),
               OpcUaCommandType.BrowseChildNodesFromRoot => new BrowseChildNodesFromRootResponse(),
               _ => default
           };
        }
    }
}