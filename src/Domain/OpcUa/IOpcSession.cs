// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using Opc.Ua;
using Opc.Ua.Client;
using BrowseRequest = OMP.Connector.Domain.Schema.Request.Control.BrowseRequest;
using BrowseResponse = OMP.Connector.Domain.Schema.Responses.Control.BrowseResponse;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IOpcSession : IDisposable
    {
        Session Session { get; }

        Task ConnectAsync(EndpointDescription endpointDescription);

        Task ConnectAsync(string opcUaServerUrl);

        Task UseAsync(Action<Session, IComplexTypeSystem> action);

        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);

        TNode GetNode<TNode>(NodeId nodeId) where TNode : class;

        Task<Type> LoadTypeAsync(NodeId nodeId);

        IEnumerable<Subscription> Subscriptions { get; }

        Task<IEnumerable<BrowseResponse>> BrowseNodesAsync(
            IEnumerable<(NodeId NodeId, BrowseRequest Command)> nodeIdCommands,
            Func<BrowseRequest, BrowseResponse> constructResultFunc);

        void ReadNodes(List<NodeId> nodeIds, int batchSize, List<object> values, List<ServiceResult> errors);

        StatusCodeCollection WriteNodes(WriteValueCollection writeValues);

        void ConvertToOpcUaTypedValues(IEnumerable<WriteRequestWrapper> writeValues);
    }
}