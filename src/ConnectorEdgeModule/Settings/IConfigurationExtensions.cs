using Microsoft.Extensions.Configuration;

namespace OMP.Connector.EdgeModule.Settings
{
    public static class IConfigurationExtensions
    {
        public const string RootDirectoryKey = "RootDirectory";
        public static string GetRouteDirectory(this IConfiguration configuration)
        {
            return configuration[RootDirectoryKey];
        }
    }
}
