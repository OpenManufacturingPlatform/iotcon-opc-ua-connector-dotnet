using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Extensions;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Request.Discovery;
using OMP.Connector.Domain.Schema.Responses.Discovery;
using BrowseChildNodesRequest = OMP.Connector.Domain.Schema.Request.Discovery.BrowseChildNodesRequest;
using BrowseChildNodesResponse = OMP.Connector.Domain.Schema.Responses.Discovery.BrowseChildNodesResponse;
using ServerDiscoveryResponse = OMP.Connector.Domain.Schema.Responses.Discovery.ServerDiscoveryResponse;

namespace OMP.Connector.Application.Services
{
    public class DiscoveryService : IDiscoveryService
    {
        private readonly ILogger _logger;
        private readonly string _schemaUrl;
        private readonly IMapper _mapper;
        private readonly IDiscoveryProvider _discoveryProvider;
        private readonly ISessionPoolStateManager _sessionPoolManager;
        private readonly IEndpointDescriptionRepository _endpointDescriptionRepository;

        public DiscoveryService(
            IDiscoveryProvider discoveryProvider,
            ISessionPoolStateManager sessionPoolStateManager,
            IEndpointDescriptionRepository dataManagementService,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMapper mapper,
            ILogger<DiscoveryService> logger
            )
        {
            this._mapper = mapper;
            this._logger = logger;

            this._schemaUrl = connectorConfiguration.Value.Communication.SchemaUrl;
            this._discoveryProvider = discoveryProvider;
            this._sessionPoolManager = sessionPoolStateManager;
            this._endpointDescriptionRepository = dataManagementService;
        }

        public async Task<ServerDiscoveryResponse> ExecuteAsync(string endpointUrl)
        {
            var url = new UriBuilder(endpointUrl);
            var endpointAddress = $"opc.tcp://{url.Host}:{url.Port}";
            var deviceExists = await IpAddressExistsAsync(url.Host);

            if (!deviceExists)
                throw new ApplicationException($"Ip Address: [{url.Host} is not reachable.");

            this._logger.Information($"Starting discovery of Server: [{endpointAddress}]");

            var connected = await this.ConnectToOpcServerAsync(endpointAddress);

            if (!connected)
                throw new ApplicationException($"Unable to discover OPC UA server at endpoint [{endpointAddress}]");

            this._logger.Debug($"Successfully discovered OPC UA server at endpoint [{endpointAddress}]");

            return new ServerDiscoveryResponse
            {
                OpcUaCommandType = OpcUaCommandType.ServerDiscovery,
                Message = "Good"
            };
        }

        public async Task<ICommandResponse> ExecuteNodeDiscoveryAsync(CommandRequest commandRequest,
            BrowseChildNodesRequest request, string endpointUrl)
        {
            var opcSession =
                await this._sessionPoolManager.GetSessionAsync(endpointUrl, new System.Threading.CancellationToken());
            var browsedNode = await this._discoveryProvider.DiscoverChildNodesAsync(opcSession, request);

            var response = new BrowseChildNodesResponse
            {
                OpcUaCommandType = OpcUaCommandType.BrowseChildNodes,
                DiscoveredOpcNode = this._mapper.Map<DiscoveredOpcNode>(browsedNode),
                Message = "Good"
            };

            return response;
        }

        public async Task<ICommandResponse> ExecuteRootNodeDiscoveryAsync(CommandRequest commandRequest,
            BrowseChildNodesFromRootRequest rootNodeDiscovery, string endpointUrl)
        {
            var opcSession =
                await this._sessionPoolManager.GetSessionAsync(endpointUrl, new System.Threading.CancellationToken());
            var nodes = await this._discoveryProvider.DiscoverRootNodesAsync(opcSession,
                int.Parse(rootNodeDiscovery.BrowseDepth));

            var response = new BrowseChildNodesFromRootResponse
            {
                OpcUaCommandType = OpcUaCommandType.BrowseChildNodesFromRoot,
                Message = "Good",
                DiscoveredOpcNodes = this._mapper.Map<IEnumerable<DiscoveredOpcNode>>(nodes)
            };

            return response;
        }

        public async Task<CommandResponse> ExecuteAsync(CommandRequest commandRequest)
        {
            var responses = new List<ICommandResponse>();
            CommandResponse responseMessage = default;
            try
            {
                foreach (var discoveryRequest in commandRequest.Payload.Requests)
                {
                    switch (discoveryRequest)
                    {
                        case ServerDiscoveryRequest serverDiscoveryRequest:
                            responses.Add(await this.ExecuteServerDiscoveryAsync(commandRequest, serverDiscoveryRequest));
                            break;
                        case BrowseChildNodesRequest nodeDiscoveryRequest:
                            responses.Add(await this.ExecuteNodeDiscoveryAsync(commandRequest, nodeDiscoveryRequest,
                                commandRequest.GetEndpointUrl()));
                            break;
                        case BrowseChildNodesFromRootRequest rootNodeDiscoveryRequest:
                            responses.Add(await this.ExecuteRootNodeDiscoveryAsync(commandRequest, rootNodeDiscoveryRequest,
                                commandRequest.GetEndpointUrl()));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                responseMessage = CommandResponseCreator.GetErrorResponseMessage(this._schemaUrl, commandRequest);
                this._logger.Error(e.Demystify());
            }

            responseMessage ??= CommandResponseCreator.GetCommandResponseMessage(this._schemaUrl, commandRequest, responses);
            return responseMessage;
        }

        private async Task<bool> ConnectToOpcServerAsync(string opcUaServerUrl)
        {
            var opcSession = await this._sessionPoolManager.GetSessionAsync(opcUaServerUrl, new System.Threading.CancellationToken());
            return opcSession != default;
        }

        private async Task<ICommandResponse> ExecuteServerDiscoveryAsync(CommandRequest commandRequest, ServerDiscoveryRequest request)
        {
            if (request == default)
                throw new ArgumentException($"{nameof(ServerDiscoveryRequest)} can not be null");

            var response = await this.ExecuteAsync(commandRequest.GetEndpointUrl());

            var endpointDescription = new EndpointDescriptionDto
            {
                EndpointUrl = commandRequest.GetEndpointUrl(),
                ServerDetails = request.ServerDetails
            };
            var endpointUpdateSuccess = this._endpointDescriptionRepository.Add(endpointDescription);
            if (!endpointUpdateSuccess)
                response.Message = "Bad: Configuration update error";

            return response;
        }

        private static async Task<bool> IpAddressExistsAsync(string ipAddress)
        {
            using var ping = new Ping();
            var pingReply = await ping.SendPingAsync(ipAddress, 5000);
            return pingReply.Status == IPStatus.Success;
        }
    }
}