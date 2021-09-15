using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OMP.Connector.Application.Services;
using Quartz;

namespace OMP.Connector.EdgeModule.Jobs
{
    public class ConfigurationRestoreJob : IJob
    {
        private readonly ConfigRestoreService _configRestoreService;
        private readonly ILogger<ConfigurationRestoreJob> _logger;

        public ConfigurationRestoreJob(ConfigRestoreService configRestoreService, ILogger<ConfigurationRestoreJob> logger)
        {
            this._configRestoreService = configRestoreService;
            this._logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                return this._configRestoreService.RestoreConfigurationAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                var error = ex.Demystify();
                this._logger.LogError(error, "Error occurred while attempting to [re]apply configuration");
            }

            return Task.CompletedTask; //?? Maybe just propogate the error up and see what quartz does
        }
    }
}