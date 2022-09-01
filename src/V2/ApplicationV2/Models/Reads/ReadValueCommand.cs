// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Reads
{
    public record ReadValueCommand
    {
        public NodeId NodeId { get; set; }
        public bool DoRegisteredRead { get; set; }

        public ReadValueCommand(NodeId nodeId, bool doRegisteredRead = false)
        {
            this.NodeId = nodeId;
            this.DoRegisteredRead = doRegisteredRead;
        }
    }
}
