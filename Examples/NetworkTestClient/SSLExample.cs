using Network;
using Network.Secure;
using System;
using TestServerClientPackets;

namespace NetworkTestClient
{
    class SSLExample
    {
        public async void Demo()
        {
            //1. Load the secure connection configuration. Create default configuration: ConnectionFactory.CreateSecureConnectionConfiguration();
            SecureConnectionConfiguration secureConnectionConfiguration = SecureConnectionConfigurationHelper.GetConfiguration();
            //2. Load client certificate including the public key.
            secureConnectionConfiguration.LoadCertificates("certificate.pfx", "psw");

            //3. Connect to the secure server.
            ConnectionResult connectionResult = ConnectionResult.CertificateError;
            TcpSecureConnection tcpSecureConnection = ConnectionFactory.CreateTcpSecureConnection(ref secureConnectionConfiguration, "127.0.0.1", 1235, out connectionResult);
            if (connectionResult != ConnectionResult.Connected)
                return;

            //4. Unlock the remote connection. (Required, because there is no ClientConnectionContainer)
            tcpSecureConnection.UnlockRemoteConnection();

            //5. Send Request and receive the response from the secure server.
            CalculationResponse calculationResponse = await tcpSecureConnection.SendAsync<CalculationResponse>(new CalculationRequest(10, 10));
            Console.WriteLine($"Answer received {calculationResponse.Result}");
        }
    }
}
