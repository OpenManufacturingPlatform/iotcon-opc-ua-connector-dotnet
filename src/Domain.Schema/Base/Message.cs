using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Attributes.Examples;
using OMP.Connector.Domain.Schema.Attributes.Regex;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.MetaData.Message;

namespace OMP.Connector.Domain.Schema.Base
{
    public abstract class Message<TPayload> : Model<MessageMetaData>, IMessage
    {
        [MessageNameSpace]
        [NamespaceExamples]
        [JsonProperty("namespace", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [Description("The Namespace of the message")]
        public override string Namespace { get; set; }

        [JsonProperty("payload", Required = Required.Always)]
        [Description("Message content. Needs to be specified in schemas derived from this envelope schema")]
        public TPayload Payload { get; set; }
    }
}