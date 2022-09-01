// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using ApplicationV2.Models.Browse;
using ApplicationV2.Models.Call;
using ApplicationV2.Models.Reads;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models.Writes;
using ApplicationV2.Services;
using ApplicationV2.Sessions;
using ApplicationV2.Sessions.SessionManagement;
using Microsoft.Extensions.Logging;
using OneOf;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2
{
    public class OmpOpcUaClient : IOmpOpcUaClient
    {
        #region [Fields]
        private readonly ISessionPoolStateManager sessionPoolStateManager;
        private readonly IWriteCommandService writeCommandService;
        private readonly IReadCommandService readCommandService;
        private readonly ISubscriptionCommandService subscriptionCommandsService;
        private readonly ICallCommandService callCommandService;
        private readonly ILogger<OmpOpcUaClient> logger; 
        #endregion

        #region [Ctor]
        public OmpOpcUaClient(
            ISessionPoolStateManager sessionPoolStateManager,
            IWriteCommandService writeCommandService,
            IReadCommandService readCommandService,
            ISubscriptionCommandService subscriptionCommandsService,
            ICallCommandService callCommandService,
            ILogger<OmpOpcUaClient> logger
            )
        {
            this.sessionPoolStateManager = sessionPoolStateManager;
            this.writeCommandService = writeCommandService;
            this.readCommandService = readCommandService;
            this.subscriptionCommandsService = subscriptionCommandsService;
            this.callCommandService = callCommandService;
            this.logger = logger;
        }
        #endregion

        #region [Browse]
        [Obsolete("Please use ReadNodesAsync")]
        public Task<OneOf<ReadNodeCommandResponseCollection, Exception>> BrowseNodesAsync(BrowseCommandCollection commands, CancellationToken cancellationToken)
        {
            var commandCollection = new ReadNodeCommandCollection(commands.EndpointUrl);
            commandCollection.AddRange(commands);
            return ReadNodesAsync(commandCollection, cancellationToken);
        } 
        #endregion

        #region [Call]

        public async Task<OneOf<CallCommandCollectionResponse, Exception>> CallNodesAsync(CallCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(commands.EndpointUrl, cancellationToken);
                return await callCommandService.CallNodesAsync(opcUaSession, commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Call command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        #endregion

        #region [Read]
        public async Task<OneOf<ReadNodeCommandResponseCollection, Exception>> ReadNodesAsync(ReadNodeCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(commands.EndpointUrl, cancellationToken);
                return await readCommandService.ReadNodesAsync(opcUaSession, commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the reading of nodes command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<ReadValueResponseCollection, Exception>> ReadValuesAsync(ReadValueCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(commands.EndpointUrl, cancellationToken);
                return await readCommandService.ReadValuesAsync(opcUaSession, commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the reading of values command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        #endregion

        #region [Subscriptions]
        public async Task<OneOf<CreateSubscriptionResponse, Exception>> CreateSubscriptions(CreateSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await subscriptionCommandsService.CreateSubscriptions(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the subscription creation: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<RemoveAllSubscriptionsResponse, Exception>> RemoveAllSubscriptions(RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await subscriptionCommandsService.RemoveAllSubscriptions(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while removing allsubscriptions: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<RemoveSubscriptionsResponse, Exception>> RemoveSubscriptionsCommand(RemoveSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await subscriptionCommandsService.RemoveSubscriptionsCommand(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the subscription removal: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }
        #endregion

        #region [Write]
        public async Task<OneOf<WriteResponseCollection, Exception>> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(commands.EndpointUrl, cancellationToken);
                return await writeCommandService.WriteAsync(opcUaSession, commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the write command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        } 
        #endregion

        protected virtual Task<IOpcUaSession> GetSession(string endpointUrl, CancellationToken cancellationToken)
            => sessionPoolStateManager.GetSessionAsync(endpointUrl, cancellationToken);
        
    }
}
