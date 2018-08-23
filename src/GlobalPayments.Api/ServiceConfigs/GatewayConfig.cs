﻿using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Gateways;
using System;

namespace GlobalPayments.Api {
    public class GatewayConfig : Configuration {
        // Portico
        /// <summary>
        /// Account's site ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Account's license ID
        /// </summary>
        public int LicenseId { get; set; }

        /// <summary>
        /// Account's device ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Account's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Account's password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Integration's developer ID
        /// </summary>
        /// <remarks>
        /// This is provided at the start of an integration's certification.
        /// </remarks>
        public string DeveloperId { get; set; }

        /// <summary>
        /// Integration's version number
        /// </summary>
        /// <remarks>
        /// This is provided at the start of an integration's certification.
        /// </remarks>
        public string VersionNumber { get; set; }

        /// <summary>
        /// Account's secret API key
        /// </summary>
        public string SecretApiKey { get; set; }

        /// <summary>
        /// A unique device id to to be passed with each transaction
        /// </summary>
        public string UniqueDeviceId { get; set; }

        // Realex
        /// <summary>
        /// Account's account ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Account's merchant ID
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Account's rebate password
        /// </summary>
        public string RebatePassword { get; set; }

        /// <summary>
        /// Account's refund password
        /// </summary>
        public string RefundPassword { get; set; }

        /// <summary>
        /// Account's shared secret
        /// </summary>
        public string SharedSecret { get; set; }

        /// <summary>
        /// Channel for an integration's transactions (e.g. "internet")
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Hosted Payment Page (HPP) configuration
        /// </summary>
        public HostedPaymentConfig HostedPaymentConfig { get; set; }

        /// <summary>
        /// Client Id for Global Payments Data Services
        /// </summary>
        public string DataClientId { get; set; }

        /// <summary>
        /// Client Secret for Global Payments Data Services
        /// </summary>
        public string DataClientSecret { get; set; }

        /// <summary>
        /// The UserId for the Global Payment Data Services
        /// </summary>
        public string DataClientUserId { get; set; }

        /// <summary>
        /// The Url of the Global Data Service
        /// </summary>
        public string DataClientSeviceUrl { get; set; }

        internal override void ConfigureContainer(ConfiguredServices services) {
            if (!string.IsNullOrEmpty(MerchantId)) {
                var gateway = new RealexConnector {
                    AccountId = AccountId,
                    Channel = Channel,
                    MerchantId = MerchantId,
                    RebatePassword = RebatePassword,
                    RefundPassword = RefundPassword,
                    SharedSecret = SharedSecret,
                    Timeout = Timeout,
                    ServiceUrl = ServiceUrl,
                    HostedPaymentConfig = HostedPaymentConfig
                };
                services.GatewayConnector = gateway;
                services.RecurringConnector = gateway;

                var reportingService = new DataServicesConnector {
                    ClientId = DataClientId,
                    ClientSecret = DataClientSecret,
                    UserId = DataClientUserId,
                    ServiceUrl = DataClientSeviceUrl ?? "https://globalpay-test.apigee.net/apis/reporting/",
                    Timeout = Timeout
                };
                services.ReportingService = reportingService;
            }
            else {
                var gateway = new PorticoConnector {
                    SiteId = SiteId,
                    LicenseId = LicenseId,
                    DeviceId = DeviceId,
                    Username = Username,
                    Password = Password,
                    SecretApiKey = SecretApiKey,
                    DeveloperId = DeveloperId,
                    VersionNumber = VersionNumber,
                    Timeout = Timeout,
                    ServiceUrl = ServiceUrl + "/Hps.Exchange.PosGateway/PosGatewayService.asmx",
                    UniqueDeviceId = UniqueDeviceId
                };
                services.GatewayConnector = gateway;

                if (!string.IsNullOrEmpty(DataClientId)) {
                    services.ReportingService = new DataServicesConnector {
                        ClientId = DataClientId,
                        ClientSecret = DataClientSecret,
                        UserId = DataClientUserId,
                        ServiceUrl = DataClientSeviceUrl ?? "https://globalpay-test.apigee.net/apis/reporting/",
                        Timeout = Timeout
                    };
                }
                else services.ReportingService = gateway;

                var payplan = new PayPlanConnector {
                    SecretApiKey = SecretApiKey,
                    Timeout = Timeout,
                    ServiceUrl = ServiceUrl + "/Portico.PayPlan.v2/"
                };
                services.RecurringConnector = payplan;
            }
        }

        internal override void Validate() {
            base.Validate();

            // portico api key
            if (!string.IsNullOrEmpty(SecretApiKey)) {
                if (SiteId != default(int) || LicenseId != default(int) || DeviceId != default(int) || !string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) {
                    throw new ConfigurationException("Configuration contains both secret api key and legacy credentials. These are mutually exclusive.");
                }
            }

            // portico legacy
            if (SiteId != default(int) || LicenseId != default(int) || DeviceId != default(int) || !string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) {
                if (!(SiteId != default(int) && LicenseId != default(int) && DeviceId != default(int) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)))
                    throw new ConfigurationException("Site, License, Device, Username and Password should all have a values for this configuration.");
            }

            // realex
            if (!string.IsNullOrEmpty(MerchantId) || !string.IsNullOrEmpty(SharedSecret)) {
                if (string.IsNullOrEmpty(MerchantId))
                    throw new ConfigurationException("MerchantId is required for this configuration.");
                else if (string.IsNullOrEmpty(SharedSecret))
                    throw new ConfigurationException("SharedSecret is required for this configuration.");
            }

            // service url
            if (string.IsNullOrEmpty(ServiceUrl)) {
                throw new ConfigurationException("Service URL could not be determined from the credentials provided. Please specify an endpoint.");
            }

            // data client
            if (!string.IsNullOrEmpty(DataClientId) || !string.IsNullOrEmpty(DataClientSecret)) {
                if (string.IsNullOrEmpty(DataClientId) || string.IsNullOrEmpty(DataClientSecret))
                    throw new ConfigurationException("Both \"DataClientID\" and \"DataClientSecret\" are required for data client services.");
                if(string.IsNullOrEmpty(DataClientUserId))
                    throw new ConfigurationException("DataClientUserId required for data client services.");
            }
        }
    }
}
