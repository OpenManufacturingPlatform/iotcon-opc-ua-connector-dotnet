// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Browse;
using OMP.PlantConnectivity.OpcUa.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    public interface IBrowseService
    {
        public const uint NodeMask = (uint)NodeClass.Object |
                                          (uint)NodeClass.Variable |
                                          (uint)NodeClass.Method |
                                          (uint)NodeClass.VariableType |
                                          (uint)NodeClass.ReferenceType |
                                          (uint)NodeClass.Unspecified;

        Task<BrowseChildNodesResponseCollection> BrowseNodes(IOpcUaSession session, CancellationToken cancellationToken, int browseDepth = 0);

        Task<BrowseChildNodesResponse> BrowseChildNodes(IOpcUaSession session, BrowseChildNodesCommand browseChildNodesCommand, CancellationToken cancellationToken);
    }
}
