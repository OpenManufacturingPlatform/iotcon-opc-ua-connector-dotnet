using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.EdgeModule
{
    public static class Program
    {
        private static string _settingsJsonFileName;

        private static async Task Main(string[] args)
        {

            _settingsJsonFileName = "moduleSettings.json";

            await CreateHostBuilder().Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                  .ConfigureAppConfiguration((hostContext, configApp) =>
                  {
                      configApp.AddEnvironmentVariables();
                      configApp.SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                      .AddJsonFile(_settingsJsonFileName, optional: true, true);
                  })
                  .ConfigureLogging((hostContext, logBuilder) =>
                  {
                      logBuilder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    //var appInsightsKey = hostContext.Configuration.GetSection("APPINSIGHTS")["INSTRUMENTATIONKEY"];
                    //if (!string.IsNullOrEmpty(appInsightsKey))
                    //    logBuilder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);

                    logBuilder.AddConsole();
                      logBuilder.AddDebug();
                  })
                  .ConfigureServices((hostingContext, services) =>
                  {
                      Bootstrapper.Bootstrap(hostingContext, services);
                  })
                  .UseDefaultServiceProvider((context, options) =>
                  {
                      options.ValidateOnBuild = true;
                  });
        }
    }
}