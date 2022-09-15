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
        public int KeepAliveIntervalInSeconds { get; set; } = 5;
        public bool DisableSubscriptionRestoreService { get; set; } = false;
        public bool DisableAlarmSubscriptionRestoreService { get; set; } = false;

        public uint NodeMask => (uint)NodeClass.Object |
                                          (uint)NodeClass.Variable |
                                          (uint)NodeClass.Method |
                                          (uint)NodeClass.VariableType |
                                          (uint)NodeClass.ReferenceType |
                                          (uint)NodeClass.Unspecified;

        public List<AuthenticationConfiguration> Authentication { get; set; } = new List<AuthenticationConfiguration>();
    }
}
