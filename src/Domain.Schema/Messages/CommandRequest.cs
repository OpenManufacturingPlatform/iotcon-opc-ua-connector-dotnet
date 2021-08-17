using System.ComponentModel;
using Omp.Connector.Domain.Schema.Base;
using Omp.Connector.Domain.Schema.Request;

namespace Omp.Connector.Domain.Schema.Messages
{
    [Description("Definition of OPC UA Command Request")]
    public class CommandRequest : Message<RequestPayload> { }
}