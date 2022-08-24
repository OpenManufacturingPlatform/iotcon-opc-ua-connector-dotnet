// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.
using ApplicationV2;
using ApplicationV2.Configuration;
using ApplicationV2.Services;
using ApplicationV2.Sessions.Auth;
using ApplicationV2.Sessions.Reconnect;
using ApplicationV2.Sessions.RegisteredNodes;
using ApplicationV2.Sessions.SessionManagement;
using ApplicationV2.Sessions.Types;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftExtensionsDependencyInjection
    {
        public static IServiceCollection AddOmpOpcUaClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Configuration
            serviceCollection.Configure<OpcUaConfiguration>(configuration.GetSection("OpcUa"));
            serviceCollection.Configure<OpcUaSettings>(configuration.GetSection("OpcUa"));
            
            //OpcUaConfiguration

            //Factories
            serviceCollection.AddSingleton<IRegisteredNodeStateManagerFactory, RegisteredNodeStateManagerFactory>();
            serviceCollection.AddTransient<IOpcUaSessionReconnectHandlerFactory, OpcUaSessionReconnectHandlerFactory>();
            serviceCollection.AddTransient<IComplexTypeSystemFactory, ComplexTypeSystemFactory>();
            

            //Services
            serviceCollection.AddTransient<IReadCommandService, ReadCommandService>();
            serviceCollection.AddTransient<IWriteCommandService, WriteCommandService>();
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
