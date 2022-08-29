// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;

namespace ApplicationV2.Configuration
{
    public class AppConfigBuilder : IAppConfigBuilder
    {
        private readonly ILogger logger;
        private readonly OpcUaClientSettings opcUaSettings;
        private readonly OmpOpcUaConfiguration opcUaConfiguration;

        public AppConfigBuilder(ILogger<AppConfigBuilder> logger, IOptions<OmpOpcUaConfiguration> opcUaConfiguration, IOptions<OpcUaClientSettings> opcUaSettings)
        {
            this.opcUaConfiguration = opcUaConfiguration.Value;
            this.opcUaSettings = opcUaSettings.Value;
            this.logger = logger;
        }

        public ApplicationConfiguration Build()
        {
            var appConfig = new ApplicationConfiguration
            {
                ApplicationType = ApplicationType.ClientAndServer,
                ApplicationName = this.opcUaSettings.ApplicationName,
                ApplicationUri = this.opcUaSettings.ApplicationUri,
                ProductUri = this.opcUaSettings.ProductUri,
                TraceConfiguration = this.GetTraceConfiguration(),
                TransportQuotas = this.GetTransportQuotas(),
                ServerConfiguration = this.GetServerConfiguration(),
                ClientConfiguration = new ClientConfiguration(),
                SecurityConfiguration = this.GetSecurityConfiguration(),
                CertificateValidator = this.GetCertificateValidator()

            };

            appConfig.CertificateValidator.Update(appConfig.SecurityConfiguration).GetAwaiter().GetResult();

            return appConfig;
        }

        private TraceConfiguration GetTraceConfiguration()
        {
            var traceConfiguration = new TraceConfiguration
            {
                TraceMasks = this.opcUaSettings.OpcStackTraceMask
            };
            traceConfiguration.ApplySettings();

            return traceConfiguration;
        }

        private TransportQuotas GetTransportQuotas()
        {
            return new TransportQuotas
            {
                MaxStringLength = this.opcUaSettings.MaxStringLength,
                MaxMessageSize = this.opcUaSettings.MaxMessageSize,
                OperationTimeout = this.opcUaConfiguration.OperationTimeoutInSeconds.ToMilliseconds()
            };
        }

        private ServerConfiguration GetServerConfiguration()
        {
            var serverConfiguration = new ServerConfiguration
            {
                MaxRegistrationInterval = this.opcUaSettings.LdsRegistrationInterval
            };
            serverConfiguration.BaseAddresses.Add(this.opcUaSettings.ServerBaseAddress);
            serverConfiguration.SecurityPolicies.Add(new ServerSecurityPolicy
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic256Sha256
            });
            return serverConfiguration;
        }

        private SecurityConfiguration GetSecurityConfiguration()
        {
            var securityConfiguration = new SecurityConfiguration
            {
                TrustedIssuerCertificates = this.GetTrustedIssuerCertificates(),
                TrustedPeerCertificates = this.GetTrustedPeerCertificates(),
                RejectedCertificateStore = this.GetRejectedCertificateStore(),
                ApplicationCertificate = this.GetApplicationCertificate(),
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024
            };

            return securityConfiguration;
        }

        private CertificateTrustList GetTrustedIssuerCertificates()
        {
            return new CertificateTrustList
            {
                StoreType = this.opcUaSettings.CertificateStoreTypeName,
                StorePath = this.opcUaSettings.IssuerCertStorePath
            };
        }

        private CertificateTrustList GetTrustedPeerCertificates()
        {
            return new CertificateTrustList
            {
                StoreType = this.opcUaSettings.CertificateStoreTypeName,
                StorePath = this.opcUaSettings.TrustedCertStorePath
            };
        }

        private CertificateTrustList GetRejectedCertificateStore()
        {
            return new CertificateTrustList
            {
                StoreType = this.opcUaSettings.CertificateStoreTypeName,
                StorePath = this.opcUaSettings.RejectedCertStorePath
            };
        }

        private CertificateIdentifier GetApplicationCertificate()
        {
            var certificateIdentifier = new CertificateIdentifier
            {
                StoreType = CertificateStoreType.X509Store,
                StorePath = this.opcUaSettings.OwnCertStorePath,
                SubjectName = this.opcUaSettings.ApplicationName
            };

            certificateIdentifier.Certificate = this.GenerateCertificate(certificateIdentifier);

            return certificateIdentifier;
        }

        private CertificateValidator GetCertificateValidator()
        {
            var certificateValidator = new CertificateValidator();
            certificateValidator.CertificateValidation += (sender, args) =>
            {
                args.Accept = true;
                var message = $"Certificate [{args.Certificate?.Subject}] was not trusted and will be forced !";
                logger.LogTrace("Certificate [{certificateSubject}] was not trusted and will be forced !", args.Certificate?.Subject);
            };

            return certificateValidator;
        }

        private X509Certificate2 GenerateCertificate(CertificateIdentifier identifier)
        {
            var certificateBuilder = CertificateFactory.CreateCertificate(
                this.opcUaSettings.ApplicationUri,
                this.opcUaSettings.ApplicationName,
                this.opcUaSettings.ApplicationName,
                default
            );
            certificateBuilder.SetLifeTime(CertificateFactory.DefaultLifeTime);
            certificateBuilder.SetRSAKeySize(CertificateFactory.DefaultKeySize);
            certificateBuilder.SetNotBefore(DateTime.UtcNow - TimeSpan.FromDays(1));
            var certificate = certificateBuilder.CreateForRSA();

            if (!certificate.HasPrivateKey)
                certificate = identifier.LoadPrivateKey(default).GetAwaiter().GetResult();

            var certificateSigningRequest = CertificateFactory.CreateSigningRequest(certificate);
            File.WriteAllBytes($"{this.opcUaSettings.ApplicationName}.csr", certificateSigningRequest);

            return certificate;
        }
    }
}
