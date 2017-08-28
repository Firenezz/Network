using Network;
using Network.Secure;
using System;
using System.Security.Authentication;

namespace TestServerClientPackets
{
    public static class SecureConnectionConfigurationHelper
    {
        public static SecureConnectionConfiguration GetConfiguration()
        {
            SecureConnectionConfiguration secureConnectionConfiguration = ConnectionFactory.CreateSecureConnectionConfiguration();
            secureConnectionConfiguration.AllowUntrustedCertificates = true; //Our self singed cert. is not trustworthy
            secureConnectionConfiguration.CheckClientRevocation = true;
            secureConnectionConfiguration.LeaveInnerStreamOpen = true;
            secureConnectionConfiguration.SslProtocol = SslProtocols.Tls12;
            return secureConnectionConfiguration;
        }
    }
}