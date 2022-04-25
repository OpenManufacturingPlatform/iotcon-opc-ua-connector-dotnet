// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OMP.Connector.EdgeModule
{
    public class MqttSample { }
    public static class Program
    {
        private static string _settingsJsonFileName;
        private static ILogger _logger;

        private static async Task Main(string[] args)
        {

            _settingsJsonFileName = "moduleSettings.json";

            try
            {
                await CreateHostBuilder().Build().RunAsync();
            }
            catch (AggregateException aex)
            {
                LogCritialError(GetErrorMessage(aex));
            }
            catch (Exception ex)
            {
                LogCritialError(GetErrorMessage(ex));                
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                  .ConfigureAppConfiguration((hostContext, configApp) =>
                  {
                      configApp.SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                      .AddJsonFile(_settingsJsonFileName, optional: true, true)
                      .AddEnvironmentVariables();
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
                      var provider = Bootstrapper.Bootstrap(hostingContext, services);
                      _logger = provider.GetService<ILogger<MqttSample>>();
                  })
                  .UseDefaultServiceProvider((context, options) =>
                  {
                      options.ValidateOnBuild = true;
                  });
        }

        private static string GetErrorMessage(AggregateException aex)
        {
            if (aex.InnerExceptions is not null && aex.InnerExceptions.Any())
                return string.Join($"{Environment.NewLine}", aex.InnerExceptions.Select(ex => GetErrorMessage(ex)).ToArray());

            return GetErrorMessage(aex);
        }

        private static string GetErrorMessage(Exception ex)
        {
            var errorMessage = ex.Message;
            if (ex.InnerException is not null)
                errorMessage = $"{errorMessage}{Environment.NewLine}{GetErrorMessage(ex.InnerException)}";

            return errorMessage;
        }

        private static void LogCritialError(string error)
        {
            error = $"Unhandled exception: {Environment.NewLine}\t{error}. {Environment.NewLine}{Environment.NewLine}Application closing";

            if (_logger is null)
                Console.WriteLine(error);
            else
                _logger.LogCritical(error);
        }
    }
}