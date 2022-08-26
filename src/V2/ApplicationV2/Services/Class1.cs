// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models;

namespace ApplicationV2.Services
{
    public interface ISubscriptionCommandsService
    {
        Task<CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>> CreateSubscriptions(IOmpOpcUaClient opcUaClient,  CreateSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<CommandResultBase> RemoveSubscriptionsCommand(IOmpOpcUaClient opcUaClient, RemoveSubscriptionsCommand command, CancellationToken cancellationToken);

        Task<CommandResultBase> RemoveAllSubscriptions(IOmpOpcUaClient opcUaClient, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
    }

    internal class SubscriptionCommandsService : ISubscriptionCommandsService
    {
        public Task<CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>> CreateSubscriptions(IOmpOpcUaClient opcUaClient, CreateSubscriptionsCommand command, CancellationToken CancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResultBase> RemoveAllSubscriptions(IOmpOpcUaClient opcUaClient, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResultBase> RemoveSubscriptionsCommand(IOmpOpcUaClient opcUaClient, RemoveSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
