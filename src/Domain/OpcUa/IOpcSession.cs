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
    public interface IOpcSession: IDisposable
    {
        Task ConnectAsync(EndpointDescription endpointDescription);

        Task ConnectAsync(string opcUaServerUrl);

        Task UseAsync(Action<Session, IComplexTypeSystem> action);

        public IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);

        public TNode GetNode<TNode>(NodeId nodeId) where TNode : class;

        public Task<Type> LoadTypeAsync(NodeId nodeId);

        public IEnumerable<Subscription> Subscriptions { get; }

        public Task<IEnumerable<BrowseResponse>> BrowseNodesAsync(
            IEnumerable<(NodeId NodeId, BrowseRequest Command)> nodeIdCommands,
            Func<BrowseRequest, BrowseResponse> constructResultFunc);

        public void ReadNodes(List<NodeId> nodeIds, int batchSize, List<object> values, List<ServiceResult> errors);

        public StatusCodeCollection WriteNodes(WriteValueCollection writeValues);

        public void ConvertToOpcUaTypedValues(IEnumerable<WriteRequestWrapper> writeValues);
    }
}