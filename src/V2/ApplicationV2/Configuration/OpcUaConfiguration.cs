// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Configuration
{
    public sealed class OpcUaConfiguration : BaseConnectorConfiguration
    {
        public int SubscriptionBatchSize { get; set; }
        public int AlarmSubscriptionBatchSize { get; set; }
        public int ReadBatchSize { get; set; }
        public int RegisterNodeBatchSize { get; set; }
        public int AwaitSessionLockTimeoutSeconds { get; set; } = 3;
        public int OperationTimeoutInSeconds { get; set; } = 120;
        public int ReconnectIntervalInSeconds { get; set; } = 10;
        public int KeepAliveIntervalInSeconds { get; set; } = 5;

        public uint NodeMask => (uint)NodeClass.Object |
                                          (uint)NodeClass.Variable |
                                          (uint)NodeClass.Method |
                                          (uint)NodeClass.VariableType |
                                          (uint)NodeClass.ReferenceType |
                                          (uint)NodeClass.Unspecified;

        public List<AuthenticationConfiguration> Authentication { get; set; } = new List<AuthenticationConfiguration>();
    }
}
