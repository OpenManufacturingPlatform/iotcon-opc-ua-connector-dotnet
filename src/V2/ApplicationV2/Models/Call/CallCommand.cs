﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Call
{
    public record CallCommand
    {
        public virtual NodeId NodeId { get; set; } = string.Empty;
        public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>(); // Klaar requirement vir Object in input arguments
    }
}
