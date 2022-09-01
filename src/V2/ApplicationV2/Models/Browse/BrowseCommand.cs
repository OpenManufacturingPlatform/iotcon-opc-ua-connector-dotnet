// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Reads;
using Opc.Ua;

namespace ApplicationV2.Models.Browse
{
    [Obsolete]
    public record BrowseCommand : ReadNodeCommand
    {
        public BrowseCommand(NodeId NodeId) : base(NodeId)
        {
        }
    }

    [Obsolete]
    public class BrowseCommandCollection : List<BrowseCommand>
    {
        public string EndpointUrl { get; set; }
        public BrowseCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}
