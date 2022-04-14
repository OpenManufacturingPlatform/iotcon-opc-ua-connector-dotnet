// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands
{
    public class WriteRequestWrapper
    {
        public string NodeId { get; set; }
        public string DataType { get; set; }
        public object Value { get; set; }
        public string RegisteredNodeId { get; set; }
    }
}
