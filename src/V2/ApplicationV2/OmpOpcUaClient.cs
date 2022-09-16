// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Models.Browse;
using OMP.PlantConnectivity.OpcUA.Models.Call;
using OMP.PlantConnectivity.OpcUA.Models.Discovery;
using OMP.PlantConnectivity.OpcUA.Models.Reads;
using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Models.Writes;
using OMP.PlantConnectivity.OpcUA.Serialization;
using OMP.PlantConnectivity.OpcUA.Services;
using OMP.PlantConnectivity.OpcUA.Services.Alarms;
using OMP.PlantConnectivity.OpcUA.Services.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Sessions;
using OMP.PlantConnectivity.OpcUA.Sessions.SessionManagement;
using OneOf;
using CreateSubscriptionResponse = OMP.PlantConnectivity.OpcUA.Models.Subscriptions.CreateSubscriptionResponse;

namespace OMP.PlantConnectivity.OpcUA
{
    public sealed class OmpOpcUaClient : IOmpOpcUaClient
    {
        #region [Fields]
        private readonly ISessionPoolStateManager sessionPoolStateManager;
        private readonly IWriteCommandService writeCommandService;
        private readonly IReadCommandService readCommandService;
        private readonly ISubscriptionCommandService subscriptionCommandsService;
        private readonly IAlarmSubscriptionCommandService alarmSubscriptionCommandsService;
        private readonly ICallCommandService callCommandService;
        private readonly IBrowseService browseService;
        private readonly IOmpOpcUaSerializerFactory ompOpcUaSerializerFactory;
        private readonly ILogger<OmpOpcUaClient> logger;
        #endregion

        #region [Ctor]
        public OmpOpcUaClient(
            ISessionPoolStateManager sessionPoolStateManager,
            IWriteCommandService writeCommandService,
            IReadCommandService readCommandService,
            ISubscriptionCommandService subscriptionCommandsService,
            IAlarmSubscriptionCommandService alarmSubscriptionCommandsService,
            ICallCommandService callCommandService,
            IBrowseService browseService,
            IOmpOpcUaSerializerFactory ompOpcUaSerializerFactory,
            ILogger<OmpOpcUaClient> logger
            )
        {
            this.sessionPoolStateManager = sessionPoolStateManager;
            this.writeCommandService = writeCommandService;
            this.readCommandService = readCommandService;
            this.subscriptionCommandsService = subscriptionCommandsService;
            this.alarmSubscriptionCommandsService = alarmSubscriptionCommandsService;
            this.callCommandService = callCommandService;
            this.browseService = browseService;
            this.ompOpcUaSerializerFactory = ompOpcUaSerializerFactory;
            this.logger = logger;
        }
        #endregion

        #region [Browse]
        public async Task<OneOf<BrowseChildNodesResponseCollection, Exception>> BrowseNodes(string endpointUrl, int browseDepth, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(endpointUrl, cancellationToken);
                var result = await browseService.BrowseNodes(opcUaSession, cancellationToken, browseDepth);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Call command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<BrowseChildNodesResponse, Exception>> BrowseChildNodes(BrowseChildNodesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await browseService.BrowseChildNodes(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Call command: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        [Obsolete("Please use BrowseChildNodes")]
        public Task<OneOf<BrowseChildNodesResponse, Exception>> DiscoverChildNodes(DiscoveryChildNodesCommand command, CancellationToken cancellationToken)
            => BrowseChildNodes(command, cancellationToken);

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
                logger.LogError(ex, "An error occurred while removing all subscriptions: {errorMessage}", ex.Message);
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

        #region [Alarms]
        public async Task<OneOf<CreateAlarmSubscriptionResponse, Exception>> CreateAlarmSubscriptions(CreateAlarmSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await alarmSubscriptionCommandsService.CreateAlarmSubscriptions(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the alarm subscription creation: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<RemoveAllAlarmSubscriptionsResponse, Exception>> RemoveAllAlarmSubscriptions(RemoveAllAlarmSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await alarmSubscriptionCommandsService.RemoveAllAlarmSubscriptions(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while removing all alarm subscriptions: {errorMessage}", ex.Message);
                return ex.Demystify();
            }
        }

        public async Task<OneOf<RemoveAlarmSubscriptionsResponse, Exception>> RemoveAlarmSubscriptionsCommand(RemoveAlarmSubscriptionsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var opcUaSession = await GetSession(command.EndpointUrl, cancellationToken);
                return await alarmSubscriptionCommandsService.RemoveAlarmSubscriptionsCommand(opcUaSession, command, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the alarm subscription removal: {errorMessage}", ex.Message);
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

        #region [Sessions]
        public async Task OpenSessionAsync(string endpointUrl, CancellationToken cancellationToken)
        {
            try
            {
                await GetSession(endpointUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error(s) occurred while trying to open an session on {server}: {error}", endpointUrl, ex.Message);
                throw ex.Demystify();
            }
        }

        public async Task CloseAllActiveSessionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await sessionPoolStateManager.CloseAllSessionsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error(s) occurred while trying to close session(s): {error}", ex.Message);
            }
        }



        public async Task CloseSessionAsync(string endpointUrl, CancellationToken cancellationToken)
        {
            try
            {
                await sessionPoolStateManager.CloseSessionAsync(endpointUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error(s) occurred while trying to close the session on {server}: {error}", endpointUrl, ex.Message);
            }
        }

        #endregion

        #region [Serializer]
        public async Task<OneOf<IOmpOpcUaSerializer, Exception>> GetOmpOpcUaSerializer(string endpointUrl, CancellationToken cancellationToken, bool useReversibleEncoding = true, bool useGenericEncoderOnError = true)
        {
            try
            {
                var opcUaSession = await GetSession(endpointUrl, cancellationToken);
                var toreturn = ompOpcUaSerializerFactory.Create(opcUaSession, useReversibleEncoding, useGenericEncoderOnError);
                return OneOf<IOmpOpcUaSerializer, Exception>.FromT0(toreturn);
                //return toreturn;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to get {type}: {errorMessage}", nameof(IOmpOpcUaSerializer), ex.Message);
                return ex.Demystify();
            }
        }
        #endregion

        protected Task<IOpcUaSession> GetSession(string endpointUrl, CancellationToken cancellationToken)
            => sessionPoolStateManager.GetSessionAsync(endpointUrl, cancellationToken);
    }
}
