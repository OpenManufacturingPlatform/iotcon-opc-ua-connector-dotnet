// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Threading.Tasks;
using OMP.Connector.Domain.Models.OpcUa;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Request.Discovery;

namespace OMP.Connector.Domain.Providers
{
    public interface IDiscoveryProvider
    {
        Task<IEnumerable<BrowsedNode>> DiscoverRootNodesAsync(IOpcSession opcSession, int browseDepth);

        Task<BrowsedNode> DiscoverChildNodesAsync(IOpcSession opcSession, BrowseChildNodesRequest request);
    }
}