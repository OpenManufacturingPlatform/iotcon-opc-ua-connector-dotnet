// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Browse;
using ApplicationV2.Models.Call;
using ApplicationV2.Models.Discovery;
using ApplicationV2.Models.Reads;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models.Writes;
using OneOf;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2
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
        Task<OneOf<BrowseChildNodesResponseCollection, Exception>> BrowseNodes(string endpointUrl, int browseDepth, CancellationToken cancellationToken);

        /// <summary>
        /// Browses the specified Node and it's children
        /// </summary>
        /// <param name="browseChildNodesCommand">The command containing the information of how to browse and where</param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>The browsed node  or and Exception if an error occured</returns>
        Task<OneOf<BrowseChildNodesResponse, Exception>> BrowseChildNodes(BrowseChildNodesCommand browseChildNodesCommand, CancellationToken cancellationToken);

        /// <summary>
        /// Browses the specified Node and it's children
        /// </summary>
        /// <param name="discoveryChildNodesCommand">The command containing the information of how to browse and where</param>
        /// <param name="cancellationToken">Token to signal cancellation of the process</param>
        /// <returns>The browsed/discoverd node or and Exception if an error occured</returns>
        [Obsolete("Please use BrowseChildNodes")]
        Task<OneOf<BrowseChildNodesResponse, Exception>> DiscoverChildNodes(DiscoveryChildNodesCommand discoveryChildNodesCommand, CancellationToken cancellationToken);
        #endregion

        #region [Read]
        //TODO: Write comment
        Task<OneOf<ReadValueResponseCollection, Exception>> ReadValuesAsync(ReadValueCommandCollection commands, CancellationToken cancellationToken);

        //TODO: Write comment
        Task<OneOf<ReadNodeCommandResponseCollection, Exception>> ReadNodesAsync(ReadNodeCommandCollection commands, CancellationToken cancellationToken);

        //TODO: Write comment
        [Obsolete("Please use ReadNodeAsync")]
        Task<OneOf<ReadNodeCommandResponseCollection, Exception>> BrowseNodesAsync(BrowseCommandCollection commands, CancellationToken cancellationToken);
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
        Task<OneOf<CreateSubscriptionResponse, Exception>> CreateSubscriptions(CreateSubscriptionsCommand command, CancellationToken CancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveSubscriptionsResponse, Exception>> RemoveSubscriptionsCommand(RemoveSubscriptionsCommand command, CancellationToken cancellationToken);

        //TODO: Write comment
        Task<OneOf<RemoveAllSubscriptionsResponse, Exception>> RemoveAllSubscriptions(RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
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
    }
}
