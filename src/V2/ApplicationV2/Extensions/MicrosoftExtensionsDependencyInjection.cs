// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2;
using ApplicationV2.Configuration;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Repositories;
using ApplicationV2.Services;
using ApplicationV2.Sessions.Auth;
using ApplicationV2.Sessions.Reconnect;
using ApplicationV2.Sessions.RegisteredNodes;
using ApplicationV2.Sessions.SessionManagement;
using ApplicationV2.Sessions.Types;
using ApplicationV2.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftExtensionsDependencyInjection
    {
        public static IServiceCollection AddOmpOpcUaClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Configuration
            var connectorConfiguration = new ConnectorConfiguration();
            configuration.Bind(connectorConfiguration);
            serviceCollection.Configure<ConnectorConfiguration>(configuration, options =>
            {
                configuration.Bind(options);
            });
            serviceCollection.Configure<OmpOpcUaConfiguration>(configuration.GetSection("OpcUa"));
            serviceCollection.Configure<OpcUaClientSettings>(configuration.GetSection("OpcUa"));

            //OpcUaConfiguration

            //Factories
            serviceCollection.AddSingleton<IRegisteredNodeStateManagerFactory, RegisteredNodeStateManagerFactory>();
            serviceCollection.AddTransient<IOpcUaSessionReconnectHandlerFactory, OpcUaSessionReconnectHandlerFactory>();
            serviceCollection.AddTransient<IComplexTypeSystemFactory, ComplexTypeSystemFactory>();

            //Repositories
            if (connectorConfiguration.DisableSubscriptionRestoreService)
            {
                var subscriptionRepositories = serviceCollection
                    .Where(descriptor => descriptor.ServiceType == typeof(ISubscriptionRepository))
                    .ToList();

                subscriptionRepositories.ForEach(repo =>
                {
                    serviceCollection.Remove(repo);
                });
            }
            else
            {
                serviceCollection.AddSingleton<ISubscriptionRepository, SubscriptionRepositoryInMemory>();
            }


            //Validation
            serviceCollection.AddTransient<IValidator<SubscriptionMonitoredItem>, MonitoredItemValidator>();
            //TODO: Should we not introduce more validators - 1 per method on the IOmpOpcUaClient

            //Services
            serviceCollection.AddSingleton<IBrowseService, BrowseService>();
            serviceCollection.AddTransient<ICallCommandService, CallCommandService>();
            serviceCollection.AddTransient<IReadCommandService, ReadCommandService>();
            serviceCollection.AddTransient<IWriteCommandService, WriteCommandService>();
            serviceCollection.AddTransient<ISubscriptionCommandService, SubscriptionCommandService>();
            serviceCollection.AddTransient<IMonitoredItemMessageProcessor, LoggingMonitoredItemMessageProcessor>();
            serviceCollection.AddSingleton<ISessionPoolStateManager, SessionPoolStateManager>();
            serviceCollection.AddSingleton<IUserIdentityProvider, UserIdentityProvider>();
            serviceCollection.AddSingleton<IOmpOpcUaClient, OmpOpcUaClient>();
            
            serviceCollection.AddSingleton<IAppConfigBuilder, AppConfigBuilder>();
            serviceCollection.AddSingleton(provider =>
            {
                var builder = provider.GetService<IAppConfigBuilder>();
                return builder!.Build();
            });

            //IOptions<OpcUaConfiguration> opcUaConfiguration

            return serviceCollection;
        }
    }
}
