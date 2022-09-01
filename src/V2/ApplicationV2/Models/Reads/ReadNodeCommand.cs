// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Reads
{
    public record ReadNodeCommand(NodeId NodeId);

    public class ReadNodeCommandCollection : List<ReadNodeCommand>
    {
        public string EndpointUrl { get; set; }
        public ReadNodeCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }

    public class ReadNodeCommandResponseCollection : List<CommandResult<ReadNodeCommand, Node?>> { }
}
