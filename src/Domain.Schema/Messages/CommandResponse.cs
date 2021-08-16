using System.ComponentModel;
using Omp.Connector.Domain.Schema.Base;
using Omp.Connector.Domain.Schema.Responses;

namespace Omp.Connector.Domain.Schema.Messages
{
    [Description("Definition of OPC UA Command Response")]
    public class CommandResponse : Message<ResponsePayload> { }
}