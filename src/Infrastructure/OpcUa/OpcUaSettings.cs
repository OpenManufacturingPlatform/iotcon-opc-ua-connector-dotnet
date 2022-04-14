// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel.DataAnnotations;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa
{
    public record OpcUaSettings //: IOpcUaSettings
    {
        private const string ModuleName = "OpcUaConnector";

        #region IOpcUaSettings

        public string ApplicationName { get; set; } = ModuleName;

        public string ApplicationUri { get; set; } = $"urn:{Utils.GetHostName().ToLowerInvariant()}:{ModuleName}";

        public string ProductUri { get; set; } = "https://www.bmwgroup.com/OpcUaConnector";

        public string ServerBaseAddress => $"opc.tcp://{Utils.GetHostName().ToLowerInvariant()}:4840/UA/EdgeClient";

        public int LdsRegistrationInterval { get; set; }

        public int MaxStringLength { get; set; } = 128 * 1024 - 256;

        public int MaxMessageSize { get; set; } = 4 * 1024 * 1024;

        public int OperationTimeoutMs { get; set; } = 120000;

        public bool EnableRegisteredNodes { get; set; } = false;

        [Required]
        public int DefaultServerBrowseDepth { get; set; }

        [Required]
        public int NodeBrowseDepth { get; set; }

        [Required]
        public int SubscriptionBatchSize { get; set; } = 1000;

        [Required]
        public int ReadBatchSize { get; set; } = 1000;

        [Required]
        public int RegisterNodeBatchSize { get; set; } = 1000;

        [Required]
        public string CertificateStoreTypeName { get; set; } = CertificateStoreType.Directory;

        [Required]
        public string IssuerCertStorePath { get; set; } = "pki/issuer";

        [Required]
        public string TrustedCertStorePath { get; set; } = "pki/trusted";

        [Required]
        public string RejectedCertStorePath { get; set; } = "pki/rejected";

        [Required]
        public string OwnCertStorePath { get; set; } = "CurrentUser\\UA_MachineDefault";

        public uint OpcNodeMask => (uint)NodeClass.Object |
                                   (uint)NodeClass.Variable |
                                   (uint)NodeClass.Method |
                                   (uint)NodeClass.VariableType |
                                   (uint)NodeClass.ReferenceType |
                                   (uint)NodeClass.Unspecified;

        public int OpcStackTraceMask { get; set; } = Utils.TraceMasks.Error | Utils.TraceMasks.Security | Utils.TraceMasks.StackTrace | Utils.TraceMasks.StartStop;

        public int AwaitSessionLockTimeoutSecs { get; set; }
        public int ReconnectIntervalSecs { get; set; }

        #endregion
    }
}