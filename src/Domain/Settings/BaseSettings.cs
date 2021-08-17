//using IoTPE.Logger.Abstractions.Interfaces.Settings;
//using MS = Microsoft.Extensions.Logging;

//namespace IoTPE.Connector.Domain.Settings
//{
//    public class BaseSettings : IIoTpeLoggerSettings
//    {
//        #region IIotPeLoggerSettings
//        public string ApplicationInsightsInstrumentationKey { get; set; }
//        public int ApplicationInsightsFlushTimeoutSeconds { get; set; }
//        public string EventHubLoggerConnectionString { get; set; }
//        public bool IsConsoleOutputActive { get; set; } = true;
//        public string LogLevel { get; set; } = MS.LogLevel.Debug.ToString();
//        public string ComponentName { get; set; }
//        #endregion

//        #region ISettings
//        public virtual string KeyVaultBaseUrl { get; set; }
//        public string RootDirectory { get; set; }
//        #endregion
//    }
//}
