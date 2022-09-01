// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Browse
{
    public record BrowseCommand
    {
        public virtual NodeId NodeId { get; set; } = string.Empty;
    }
}
