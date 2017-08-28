#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : NetworkTestServer
// Author           : Thomas Christof
// Created          : 28-08-2017
//
// Last Modified By : Thomas Christof
// Last Modified On : 28-08-2017
// ***********************************************************************
// <copyright>
// Company: Indie-Dev
// Thomas Christof (c) 2015
// </copyright>
// <License>
// GNU LESSER GENERAL PUBLIC LICENSE
// </License>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// ***********************************************************************
#endregion Licence - LGPLv3
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Network.Secure
{
    internal class TcpSecureClientConnection : TcpSecureConnection
    {
        internal TcpSecureClientConnection(TcpClient tcpClient, SecureConnectionConfiguration configuration) 
            : base(tcpClient, configuration)
        {

        }

        protected override SslStream InitializeSslStream(ref TcpClient tcpClient)
        {
            return new SslStream(tcpClient.GetStream(), Configuration.LeaveInnerStreamOpen, new RemoteCertificateValidationCallback(ValidateCertificate), new LocalCertificateSelectionCallback(LocalCertificateValidation));
        }

        protected override void Authenticate()
        {
            SslStream.AuthenticateAsClient(Configuration.TargetHost, Configuration.X509ClientCertificates, Configuration.SslProtocol, Configuration.CheckClientRevocation);
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return Configuration.AllowUntrustedCertificates || sslPolicyErrors == SslPolicyErrors.None;
        }

        /// <summary>
        /// Selects the local Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="targetHost">The target host.</param>
        /// <param name="localCertificates">The local certificates.</param>
        /// <param name="remoteCertificate">The remote certificate.</param>
        /// <param name="acceptableIssuers">The acceptable issuers.</param>
        /// <returns>X509Certificate.</returns>
        private X509Certificate LocalCertificateValidation(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 0)
            {
                // Use the first certificate that is from an acceptable issuer. 
                foreach (X509Certificate certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }

            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }
    }
}
