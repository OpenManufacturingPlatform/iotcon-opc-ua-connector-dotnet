// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Reads
{
    public class ReadCommandCollection : List<ReadCommand> { public string EndpointUrl { get; set; } = string.Empty; }


}
