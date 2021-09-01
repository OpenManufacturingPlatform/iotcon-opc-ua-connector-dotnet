using System.ComponentModel;
using OMP.Connector.Domain.Schema.Base;
using OMP.Connector.Domain.Schema.Request;

namespace OMP.Connector.Domain.Schema.Messages
{
    [Description("Definition of OPC UA Command Request")]
    public class CommandRequest : Message<RequestPayload> { }
}