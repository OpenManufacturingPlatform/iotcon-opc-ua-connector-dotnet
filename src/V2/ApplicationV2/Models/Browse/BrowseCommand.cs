// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Browse
{
    public record BrowseCommand
    {
        public virtual string NodeId { get; set; } = string.Empty;
    }


}
