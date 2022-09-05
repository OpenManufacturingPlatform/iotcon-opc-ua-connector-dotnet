﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Call
{
    public record NodeMethodDescribeCommand
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public virtual NodeId NodeId { get; set; } = string.Empty;
    }
}