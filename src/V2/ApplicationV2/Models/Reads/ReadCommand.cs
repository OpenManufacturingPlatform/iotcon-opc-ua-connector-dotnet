// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Reads
{
    public record ReadCommand
    {
        public string NodeId { get; set; }
        public bool DoRegisteredRead { get; set; }

        public ReadCommand(string nodeId, bool doRegisteredRead = false)
        {
            this.NodeId = nodeId;
            this.DoRegisteredRead = doRegisteredRead;
        }
    }
}
