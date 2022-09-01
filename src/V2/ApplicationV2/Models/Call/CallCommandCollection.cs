// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Call
{
    public class CallCommandCollection:List<CallCommand>
    {
        public string EndpointUrl { get; set; } = string.Empty;
    }
}
