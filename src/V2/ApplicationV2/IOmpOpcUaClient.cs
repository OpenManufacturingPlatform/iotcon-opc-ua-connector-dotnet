// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models;
using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Models.Browse;
using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Models.Reads;
using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Models.Writes;
using OMP.PlantConnectivity.OpcUA.Serialization;
using OneOf;
using Opc.Ua;
using CreateSubscriptionResponse = OMP.PlantConnectivity.OpcUA.Models.Subscriptions.CreateSubscriptionResponse;

namespace OMP.PlantConnectivity.OpcUA
{
    public interface IOmpOpcUaClient
    {
        #region [Browse]
        /// <summary>
        /// Browses for all Nodes in the server endpoind specified
        /// </summary>
        /// <param name="endpointUrl">The opcua server endpoint to browse on</param>
        /// <param name="browseDepth">How deep of a level to browse( eg. 3 levels deep). NB. This can have a major performance impact</param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>List of nodes that was found or and Exception if an error occured</returns>
        Task<OneOf<BrowseChildNodesResponseCollection, Exception>> BrowseNodesAsync(string endpointUrl, int browseDepth, CancellationToken cancellationToken);

        /// <summary>
        /// Browses the specified Node and it's children
        /// </summary>
        /// <param name="browseChildNodesCommand">The command containing the information of how to browse and where</param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>The browsed node  or and Exception if an error occured</returns>
        Task<OneOf<BrowseChildNodesResponse, Exception>> BrowseChildNodesAsync(BrowseChildNodesCommand browseChildNodesCommand, CancellationToken cancellationToken);
        #endregion

        #region [Read]
        //TODO: Write comment
        Task<OneOf<ReadValueResponseCollection, Exception>> ReadValuesAsync(ReadValueCommandCollection commands, CancellationToken cancellationToken);

        //TODO: Write comment
        Task<OneOf<ReadNodeCommandResponseCollection, Exception>> ReadNodesAsync(ReadNodeCommandCollection commands, CancellationToken cancellationToken);
        #endregion

        #region [Call]
        //TODO: Write comment
        Task<OneOf<CallCommandCollectionResponse, Exception>> CallNodesAsync(CallCommandCollection commands, CancellationToken cancellationToken);
        #endregion

        #region [Write]
        //TODO: Write comment
        Task<OneOf<WriteResponseCollection, Exception>> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken);
        #endregion

        #region [Subscriptions]

        //TODO: Write comment
        Task<OneOf<CreateSubscriptionResponse, Exception>> CreateSubscriptionsAsync(CreateSubscriptionsCommand command, CancellationToken CancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveSubscriptionsResponse, Exception>> RemoveSubscriptionsAsync(RemoveSubscriptionsCommand command, CancellationToken cancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveAllSubscriptionsResponse, Exception>> RemoveAllSubscriptionsAsync(RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
        #endregion

        #region [Alarm Subscriptions]
        //TODO: Write comment
        Task<OneOf<CreateAlarmSubscriptionResponse, Exception>> CreateAlarmSubscriptionsAsync(CreateAlarmSubscriptionsCommand command, CancellationToken CancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveAlarmSubscriptionsResponse, Exception>> RemoveAlarmSubscriptionsAsync(RemoveAlarmSubscriptionsCommand command, CancellationToken cancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveAllAlarmSubscriptionsResponse, Exception>> RemoveAllAlarmSubscriptionsAsync(RemoveAllAlarmSubscriptionsCommand command, CancellationToken cancellationToken);
        #endregion

        #region [Sessions]
        /// <summary>
        /// Opens a session for the specified endpoint. Sessions are pooled, therefore if a Session already exists for an endpoint a new Session will not be created
        /// </summary>
        /// <param name="endpointUrl">The opcua server endpoint to create an session on</param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>Task</returns>
        Task OpenSessionAsync(string endpointUrl, CancellationToken cancellationToken);

        /// <summary>
        /// Any and all Sessions to the specified endpoint will be closed
        /// </summary>
        /// <param name="endpointUrl">The opcua server endpoint upon which the session must be closed</param>
        /// <param name="cancellationToken"></param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>Task</returns>
        Task CloseSessionAsync(string endpointUrl, CancellationToken cancellationToken);

        /// <summary>
        /// Closes all Sessions in pool to any and all opc ua servers in pool
        /// </summary>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>Task</returns>
        Task CloseAllActiveSessionsAsync(CancellationToken cancellationToken);
        #endregion

        #region [Serializer]
        Task<OneOf<IOmpOpcUaSerializer, Exception>> GetOmpOpcUaSerializer(string endpointUrl, CancellationToken cancellationToken, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true);
        #endregion

        #region [Misc]
        Task<OneOf<VariableNodeDataTypeInfo, Exception>> GetVariableNodeDataTypeInfoAsync(string endpointUrl, NodeId nodeId, CancellationToken cancellationToken);
        #endregion
    }
}
