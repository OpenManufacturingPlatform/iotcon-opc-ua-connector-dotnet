// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestApplicationV2;
using Host = Microsoft.Extensions.Hosting.Host;


var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                {
                    configurationBuilder
                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logBuilder) => { })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOmpOpcUaClient(hostContext.Configuration);
                    services.AddHostedService<CommandRunner>();
                });

host.Build().Run();
