// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models;
using ApplicationV2.Models.Browse;
using ApplicationV2.Models.Call;
using ApplicationV2.Models.Reads;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models.Writes;
using OneOf;
using Opc.Ua;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2
{
    public interface IOmpOpcUaClient
    {
        Task<OneOf<ReadValueResponseCollection, Exception>> ReadValuesAsync(ReadValueCommandCollection commands, CancellationToken cancellationToken);

        Task<IEnumerable<CommandResult<BrowseCommand, Node>>> BrowseNodes(IEnumerable<BrowseCommand> commands, CancellationToken cancellationToken);

        Task<OneOf<CallCommandCollectionResponse, Exception>> CallNodesAsync(CallCommandCollection commands, CancellationToken cancellationToken);

        Task<OneOf<WriteResponseCollection, Exception>> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken);

        Task<OneOf<CreateSubscriptionResponse, Exception>> CreateSubscriptions(CreateSubscriptionsCommand command, CancellationToken CancellationToken);

        Task<OneOf<RemoveSubscriptionsResponse, Exception>> RemoveSubscriptionsCommand(RemoveSubscriptionsCommand command, CancellationToken cancellationToken);

        Task<OneOf<RemoveAllSubscriptionsResponse, Exception>> RemoveAllSubscriptions(RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);

        //Task<> DiscoveryChildNodes(DiscoveryChildNodesFromRootCommand command);
        //Task</*WHAT DO WE DO HERE? CommandResult?? */> DiscoveryChildNodes(DiscoveryChildNodesCommand command);

    }
}
