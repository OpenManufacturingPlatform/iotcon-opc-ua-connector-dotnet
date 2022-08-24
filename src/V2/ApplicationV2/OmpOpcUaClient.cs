// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models;
using ApplicationV2.Models.Browse;
using ApplicationV2.Models.Call;
using ApplicationV2.Models.Reads;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models.Writes;
using ApplicationV2.Services;
using Microsoft.Extensions.Logging;
using OneOf;
using Opc.Ua;

namespace ApplicationV2
{
    public class OmpOpcUaClient : IOmpOpcUaClient
    {
        private readonly IWriteCommandService writeCommandService;
        private readonly IReadCommandService readCommandService;
        private readonly ILogger<OmpOpcUaClient> logger;

        public OmpOpcUaClient(
            IWriteCommandService writeCommandService,
            IReadCommandService readCommandService,
            ILogger<OmpOpcUaClient> logger
            )
        {
            this.writeCommandService = writeCommandService;
            this.readCommandService = readCommandService;
            this.logger = logger;
        }

        public Task<IEnumerable<CommandResult<BrowseCommand, Node>>> BrowseNodes(IEnumerable<BrowseCommand> commands, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CommandResult<CallCommand, IEnumerable<CallOutputArguments>>>> CallNodes(IEnumerable<CallCommand> commands, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>> CreateSubscriptions(CreateSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OneOf<ReadResponseCollection, Exception>> ReadValuesAsync(ReadCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                return await readCommandService.ReadValuesAsync(commands, cancellationToken);
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

        public async Task<OneOf<WriteResponseCollection, Exception>> WriteAsync(WriteCommandCollection commands, CancellationToken cancellationToken)
        {
            try
            {
                return await writeCommandService.WriteAsync(commands, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the write command: {errorMessage}", ex.Message);
                return ex;
            }
        }
    }

}
