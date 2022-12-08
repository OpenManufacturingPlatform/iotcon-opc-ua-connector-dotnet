// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Models.Call
{
    public record NodeMethodDescribeResponse
    {
        public virtual NodeId ObjectId { get; set; } = string.Empty;
        public virtual NodeId MethodId { get; set; } = string.Empty;

        public IReadOnlyCollection<Argument> InputArguments { get; set; } = new List<Argument>();
        public IReadOnlyCollection<Argument> OutArguments { get; set; } = new List<Argument>();
    }
}
