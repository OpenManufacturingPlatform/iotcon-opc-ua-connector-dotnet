// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Sessions
{
    public interface IOpcUaSession : IDisposable
    {
        #region [Misc]
        string GetBaseEndpointUrl();
        NamespaceTable? GetNamespaceUris();
        IServiceMessageContext? GetServiceMessageContext();
        #endregion

        #region [Connection]
        Task ConnectAsync(string opcUaServerUrl);
        Task ConnectAsync(EndpointDescription endpointDescription);
        Task DisconnectAsync(CancellationToken stoppingToken);
        #endregion

        #region [Call]
        Task<IEnumerable<NodeMethodDescribeResponse>> GetMethodInfoListAsync(IEnumerable<NodeId> nodeIds, CancellationToken cancellationToken);
        Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(NodeMethodDescribeCommand command, CancellationToken cancellationToken);
        Task<NodeMethodDescribeResponse> GetNodeMethodArgumentsAsync(NodeId nodeId, CancellationToken cancellationToken);
        Task<CallResponse> CallAsync(IEnumerable<CallMethodRequest> callMethodRequests, CancellationToken? cancellationToken = null);
        Task<CallResponse> CallAsync(CallMethodRequestCollection callMethodRequestCollection, CancellationToken? cancellationToken = null);
        #endregion

        #region [Browse]
        Task<ReferenceDescriptionCollection> BrowseAsync(BrowseDescription browseDescription, CancellationToken? cancellationToken = null);
        #endregion

        #region [Write]
        ResponseHeader WriteNodes(WriteValueCollection writeValues, out StatusCodeCollection statusCodeCollection);
        #endregion

        #region [Read]
        Node? ReadNode(NodeId nodeId);
        List<object> ReadNodeValues(List<NodeId> nodeIds, out List<ServiceResult> errors);

        string GetNodeFriendlyDataType(NodeId dataTypeNodeId, int valueRank);
        #endregion

        #region [Registered Nodes]
        void RestoreRegisteredNodeIds();
        ResponseHeader RegisterNodes(NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
        ResponseHeader RegisterNodes(RequestHeader requestHeader, NodeIdCollection nodesToRegister, out NodeIdCollection registeredNodeIds);
        IEnumerable<KeyValuePair<string, NodeId>> GetRegisteredNodeIds(IEnumerable<string> nodeIds);
        #endregion

        #region [Subscriptions]
        Subscription CreateOrUpdateSubscription(SubscriptionMonitoredItem monitoredItem, bool autoApplyChanges = false);
        Subscription CreateOrUpdateAlarmSubscription(AlarmSubscriptionMonitoredItem monitoredItem, bool autoApplyChanges = false);
        void ActivatePublishingOnAllSubscriptions();
        void RefreshAlarmsOnAllSubscriptions();
        IEnumerable<Subscription> GetSubscriptions();
        Task<bool> RemoveSubscriptionAsync(Subscription subscription);
        Task<bool> RemoveSubscriptionsAsync(IEnumerable<Subscription> subscriptions);
        #endregion
    }
}
