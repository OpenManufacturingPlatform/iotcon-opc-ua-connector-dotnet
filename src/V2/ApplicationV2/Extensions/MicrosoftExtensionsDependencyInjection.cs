// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using FluentValidation;
using Microsoft.Extensions.Configuration;
using OMP.Connector.Application.Validators;
using OMP.PlantConnectivity.OpcUa;
using OMP.PlantConnectivity.OpcUa.Configuration;
using OMP.PlantConnectivity.OpcUa.Models.Alarms;
using OMP.PlantConnectivity.OpcUa.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUa.Repositories;
using OMP.PlantConnectivity.OpcUa.Serialization;
using OMP.PlantConnectivity.OpcUa.Services;
using OMP.PlantConnectivity.OpcUa.Services.Alarms;
using OMP.PlantConnectivity.OpcUa.Services.Subscriptions;
using OMP.PlantConnectivity.OpcUa.Sessions.Auth;
using OMP.PlantConnectivity.OpcUa.Sessions.Reconnect;
using OMP.PlantConnectivity.OpcUa.Sessions.RegisteredNodes;
using OMP.PlantConnectivity.OpcUa.Sessions.SessionManagement;
using OMP.PlantConnectivity.OpcUa.Sessions.Types;
using OMP.PlantConnectivity.OpcUa.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftExtensionsDependencyInjection
    {

        public static IServiceCollection AddOmpOpcUaClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOmpOpcUaClient<LoggingMonitoredItemMessageProcessor, SubscriptionRepositoryInMemory, LoggingAlarmMonitoredItemMessageProcessor, AlarmSubscriptionRepositoryInMemory>(configuration);
            return serviceCollection;
        }

        public static IServiceCollection AddOmpOpcUaClient<TProcessor, TSubscriptionRepo, TAlarmProcessor, TAlarmSubscriptionRepo>(this IServiceCollection serviceCollection, IConfiguration configuration)
            where TProcessor : class, IMonitoredItemMessageProcessor
            where TSubscriptionRepo : class, ISubscriptionRepository
            where TAlarmProcessor : class, IAlarmMonitoredItemMessageProcessor
            where TAlarmSubscriptionRepo : class, IAlarmSubscriptionRepository
        {
            //Configuration
            var connectorConfiguration = new OmpOpcUaConfiguration();
            configuration.Bind(connectorConfiguration);
            serviceCollection.Configure<OmpOpcUaConfiguration>(configuration.GetSection("OpcUa"), options =>
            {
                configuration.GetSection("OpcUa").Bind(options);
            });
            serviceCollection.Configure<OmpOpcUaConfiguration>(configuration.GetSection("OpcUa"));
            serviceCollection.Configure<OpcUaClientSettings>(configuration.GetSection("OpcUa"));

            //OpcUaConfiguration

            //Factories
            serviceCollection.AddSingleton<IRegisteredNodeStateManagerFactory, RegisteredNodeStateManagerFactory>();
            serviceCollection.AddTransient<IOpcUaSessionReconnectHandlerFactory, OpcUaSessionReconnectHandlerFactory>();
            serviceCollection.AddTransient<IComplexTypeSystemFactory, ComplexTypeSystemFactory>();
            serviceCollection.AddTransient<IOmpOpcUaSerializerFactory, OpcUaNewtonsoftSerializerFactory>();

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
                serviceCollection.AddSingleton<ISubscriptionRepository, TSubscriptionRepo>();
            }

            if (connectorConfiguration.DisableAlarmSubscriptionRestoreService)
            {
                var alarmSubscriptionRepositories = serviceCollection
                    .Where(descriptor => descriptor.ServiceType == typeof(IAlarmSubscriptionRepository))
                    .ToList();

                alarmSubscriptionRepositories.ForEach(repo =>
                {
                    serviceCollection.Remove(repo);
                });
            }
            else
            {
                serviceCollection.AddSingleton<IAlarmSubscriptionRepository, TAlarmSubscriptionRepo>();
            }


            //Validation
            serviceCollection.AddTransient<IValidator<SubscriptionMonitoredItem>, MonitoredItemValidator>();
            serviceCollection.AddTransient<IValidator<AlarmSubscriptionMonitoredItem>, AlarmMonitoredItemValidator>();
            //TODO: Should we not introduce more validators - 1 per method on the IOmpOpcUaClient

            //Services
            serviceCollection.AddSingleton<IBrowseService, BrowseService>();
            serviceCollection.AddTransient<ICallCommandService, CallCommandService>();
            serviceCollection.AddTransient<IReadCommandService, ReadCommandService>();
            serviceCollection.AddTransient<IWriteCommandService, WriteCommandService>();
            serviceCollection.AddTransient<ISubscriptionCommandService, SubscriptionCommandService>();
            serviceCollection.AddTransient<IAlarmSubscriptionCommandService, AlarmSubscriptionCommandService>();
            serviceCollection.AddSingleton<ISessionPoolStateManager, SessionPoolStateManager>();
            serviceCollection.AddSingleton<IUserIdentityProvider, UserIdentityProvider>();
            serviceCollection.AddSingleton<IOmpOpcUaClient, OmpOpcUaClient>();

            serviceCollection.AddSingleton<IAppConfigBuilder, AppConfigBuilder>();
            serviceCollection.AddSingleton(provider =>
            {
                var builder = provider.GetService<IAppConfigBuilder>();
                return builder!.Build();
            });

            // Monitored Item Notifaction Processor
            serviceCollection.AddTransient<IMonitoredItemMessageProcessor, TProcessor>();

            // Alarm Notifaction Processor
            serviceCollection.AddTransient<IAlarmMonitoredItemMessageProcessor, TAlarmProcessor>();

            return serviceCollection;
        }
    }
}
