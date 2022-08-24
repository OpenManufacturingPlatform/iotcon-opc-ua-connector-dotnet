// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Reads
{
    public record ReadCommand
    {
        public bool DoRegisteredRead { get; set; } = false;
        public virtual string NodeId { get; set; } = string.Empty;
    }


}
