// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using OMP.Connector.Domain.Schema.Base;
using OMP.Connector.Domain.Schema.Responses;

namespace OMP.Connector.Domain.Schema.Messages
{
    [Description("Definition of OPC UA Command Response")]
    public class CommandResponse : Message<ResponsePayload> { }
}