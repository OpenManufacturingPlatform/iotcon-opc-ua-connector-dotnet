using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Extensions;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ConfigBuilders
{
    public class AppConfigBuilder : IAppConfigBuilder
    {
        private readonly ILogger _logger;
        private readonly OpcUaSettings _opcUaSettings;

        public AppConfigBuilder(ILogger<AppConfigBuilder> logger, IOptions<ConnectorConfiguration> connectorConfiguration)
        {
            this._opcUaSettings = connectorConfiguration.Value.OpcUa.GetConfig<OpcUaSettings>();
            this._logger = logger;
        }

        public ApplicationConfiguration Build()
        {
            var appConfig = new ApplicationConfiguration
            {
                ApplicationType = ApplicationType.ClientAndServer,
                ApplicationName = this._opcUaSettings.ApplicationName,
                ApplicationUri = this._opcUaSettings.ApplicationUri,
                ProductUri = this._opcUaSettings.ProductUri,
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
                TraceMasks = this._opcUaSettings.OpcStackTraceMask
            };
            traceConfiguration.ApplySettings();

            return traceConfiguration;
        }

        private TransportQuotas GetTransportQuotas()
        {
            return new TransportQuotas
            {
                MaxStringLength = this._opcUaSettings.MaxStringLength,
                MaxMessageSize = this._opcUaSettings.MaxMessageSize,
                OperationTimeout = this._opcUaSettings.OperationTimeoutMs
            };
        }

        private ServerConfiguration GetServerConfiguration()
        {
            var serverConfiguration = new ServerConfiguration
            {
                MaxRegistrationInterval = this._opcUaSettings.LdsRegistrationInterval
            };
            serverConfiguration.BaseAddresses.Add(this._opcUaSettings.ServerBaseAddress);
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
                StoreType = this._opcUaSettings.CertificateStoreTypeName,
                StorePath = this._opcUaSettings.IssuerCertStorePath
            };
        }

        private CertificateTrustList GetTrustedPeerCertificates()
        {
            return new CertificateTrustList
            {
                StoreType = this._opcUaSettings.CertificateStoreTypeName,
                StorePath = this._opcUaSettings.TrustedCertStorePath
            };
        }

        private CertificateTrustList GetRejectedCertificateStore()
        {
            return new CertificateTrustList
            {
                StoreType = this._opcUaSettings.CertificateStoreTypeName,
                StorePath = this._opcUaSettings.RejectedCertStorePath
            };
        }

        private CertificateIdentifier GetApplicationCertificate()
        {
            var certificateIdentifier = new CertificateIdentifier
            {
                StoreType = CertificateStoreType.X509Store,
                StorePath = this._opcUaSettings.OwnCertStorePath,
                SubjectName = this._opcUaSettings.ApplicationName
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
                this._logger.Trace(message);
            };

            return certificateValidator;
        }

        private X509Certificate2 GenerateCertificate(CertificateIdentifier identifier)
        {
            var certificateBuilder = CertificateFactory.CreateCertificate(
                this._opcUaSettings.ApplicationUri,
                this._opcUaSettings.ApplicationName,
                this._opcUaSettings.ApplicationName,
                default
            );
            certificateBuilder.SetLifeTime(CertificateFactory.DefaultLifeTime);
            certificateBuilder.SetRSAKeySize(CertificateFactory.DefaultKeySize);
            certificateBuilder.SetNotBefore(DateTime.UtcNow - TimeSpan.FromDays(1));
            var certificate = certificateBuilder.CreateForRSA();

            if (!certificate.HasPrivateKey)
                certificate = identifier.LoadPrivateKey(default).GetAwaiter().GetResult();

            var certificateSigningRequest = CertificateFactory.CreateSigningRequest(certificate);
            File.WriteAllBytes($"{this._opcUaSettings.ApplicationName}.csr", certificateSigningRequest);

            return certificate;
        }
    }
}