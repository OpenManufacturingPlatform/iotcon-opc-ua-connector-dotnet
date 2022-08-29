// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Configuration
{
    public sealed class ConnectorConfiguration
    {
        public string ConnectorId { get; set; } = string.Empty;
        public OmpOpcUaConfiguration OpcUa { get; set; } = new OmpOpcUaConfiguration();
        public bool DisableSubscriptionRestoreService { get; set; }
    }
}
