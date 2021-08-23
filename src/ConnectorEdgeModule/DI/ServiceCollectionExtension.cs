using System;
using Microsoft.Extensions.DependencyInjection;

namespace OMP.Connector.EdgeModule
{
    internal static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterDelegateFromService<TService, TDelegate>(
                this IServiceCollection serviceCollection,
                Func<TService, TDelegate> getDelegateFromService)
                where TDelegate : Delegate
        {
            return serviceCollection.AddTransient(serviceProvider =>
                getDelegateFromService(serviceProvider.GetRequiredService<TService>()));
        }
    }
}