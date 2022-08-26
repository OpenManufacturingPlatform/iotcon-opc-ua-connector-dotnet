// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading;
using ApplicationV2.Models;
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
using Opc.Ua;
using CreateSubscriptionResponse = ApplicationV2.Models.Subscriptions.CreateSubscriptionResponse;

namespace ApplicationV2
{
    public class OmpOpcUaClient : IOmpOpcUaClient
    {
        #region [Fields]
        private readonly ISessionPoolStateManager sessionPoolStateManager;
        private readonly IWriteCommandService writeCommandService;
        private readonly IReadCommandService readCommandService;
        private readonly ISubscriptionCommandsService subscriptionCommandsService;
        private readonly ILogger<OmpOpcUaClient> logger; 
        #endregion

        #region [Ctor]
        public OmpOpcUaClient(
            ISessionPoolStateManager sessionPoolStateManager,
            IWriteCommandService writeCommandService,
            IReadCommandService readCommandService,
            ISubscriptionCommandsService subscriptionCommandsService,
            ILogger<OmpOpcUaClient> logger
            )
        {
            this.sessionPoolStateManager = sessionPoolStateManager;
            this.writeCommandService = writeCommandService;
            this.readCommandService = readCommandService;
            this.subscriptionCommandsService = subscriptionCommandsService;
            this.logger = logger;
        }
        #endregion

        #region [Browse]
        public Task<IEnumerable<CommandResult<BrowseCommand, Node>>> BrowseNodes(IEnumerable<BrowseCommand> commands, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region [Call]

        public Task<IEnumerable<CommandResult<CallCommand, IEnumerable<CallOutputArguments>>>> CallNodes(IEnumerable<CallCommand> commands, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [Read]

        public async Task<OneOf<ReadResponseCollection, Exception>> ReadValuesAsync(ReadCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(commands.EndpointUrl, cancellationToken);
                return await readCommandService.ReadValuesAsync(opcUaSession, commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the read command: {errorMessage}", ex.Message);
                return ex;
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
                logger.LogError(ex, "An error occurred during the read command: {errorMessage}", ex.Message);
                return ex;
            }
        }

        public Task<CommandResultBase> RemoveAllSubscriptions(RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResultBase> RemoveSubscriptionsCommand(RemoveSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                return ex;
            }
        } 
        #endregion

        protected virtual Task<IOpcUaSession> GetSession(string endpointUrl, CancellationToken cancellationToken)
            => sessionPoolStateManager.GetSessionAsync(endpointUrl, cancellationToken);
        
    }
}
