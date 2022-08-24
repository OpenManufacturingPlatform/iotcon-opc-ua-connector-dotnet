// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Call
{
    public record CallCommand
    {
        public virtual string NodeId { get; set; } = string.Empty;
        public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>(); // Klaar requirement vir Object in input arguments
    }


}
