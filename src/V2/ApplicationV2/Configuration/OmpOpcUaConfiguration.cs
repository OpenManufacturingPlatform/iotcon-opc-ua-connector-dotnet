// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Configuration
{
    public record OmpOpcUaConfiguration
    {
        public int SubscriptionBatchSize { get; set; } = 100;
        public int AlarmSubscriptionBatchSize { get; set; } = 100;
        public int ReadBatchSize { get; set; } = 100;
        public int RegisterNodeBatchSize { get; set; } = 100;
        public int AwaitSessionLockTimeoutSeconds { get; set; } = 3;
        public int OperationTimeoutInSeconds { get; set; } = 120;
        public int ReconnectIntervalInSeconds { get; set; } = 10;
        public int SessionKeepAliveIntervalInSeconds { get; set; } = 5;
        public uint SessionTimeoutInMs { get; set; } = 100000;
        public uint SubscriptionLifetimeCountInMs { get; set; } = 100000;
        public uint SubscriptionKeepAliveCountInMs { get; set; } = 100000;
        public uint BrowseRequestedMaxReferencesPerNode { get; set; } = 200;
        public bool DisableSubscriptionRestoreService { get; set; } = false;
        public bool DisableAlarmSubscriptionRestoreService { get; set; } = false;

        public List<AuthenticationConfiguration> Authentication { get; set; } = new List<AuthenticationConfiguration>();
    }
}
