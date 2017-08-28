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
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Network.Secure
{
    /// <summary>
    /// Contains essentials values for a valid SSL communication.
    /// Used by the Server and the Client. Every property indicates:
    /// - Required or not
    /// - Addressing Server or Client
    /// </summary>
    public class SecureConnectionConfiguration
    {
        internal SecureConnectionConfiguration()
        {

        }

        /// <summary>
        /// Required: No
        /// Target: Client
        /// 
        /// [True] if no certification validation should take place.
        /// [False] if the certification validation should take place.
        /// Iff [False] "PublicKey" is required.
        /// </summary>
        public bool AllowUntrustedCertificates { get; set; } = true;

        public int ReadTimeout { get; set; } = 5000;

        public int WriteTimeout { get; set; } = 5000;

        /// <summary>
        /// Required: No
        /// Target: Client and Server.
        /// 
        /// [True] iff the inner stream (establishing network stream, without any encryption) should
        /// stay alive. Note: The inner stream won't be used after a secure connection has been established.
        /// Therefore, closing is recommended and the default value.
        /// </summary>
        public bool LeaveInnerStreamOpen { get; set; } = false;

        /// <summary>
        /// Required: No, iff "AllowUntrustedCertificates" is set to [True]
        /// Target: Client
        /// 
        /// The public key, used for the certificate validation.
        /// Only required if "AllowUntrustedCertificates" is set to [false].
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Required: No
        /// Target: Server
        /// </summary>
        public bool CheckClientRevocation { get; set; } = true;

        /// <summary>
        /// Required: No
        /// Target: Client
        /// </summary>
        public string TargetHost { get; set; } = Environment.MachineName;

        /// <summary>
        /// Required: Yes
        /// Target: Server
        /// </summary>
        public X509Certificate2 X509ServerCertificate { get; set; }

        public X509Certificate2Collection X509ClientCertificates { get; set; } = new X509Certificate2Collection();

        /// <summary>
        /// Required: No
        /// Target: Server
        /// </summary>
        public SslProtocols SslProtocol { get; set; } = SslProtocols.Tls12;

        /// <summary>
        /// Loads certificates.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="directlyInBinFolder">[True] iff the certificate is directly in the .exe folder.</param>
        /// <returns></returns>
        public void LoadCertificates(string fileName, bool directlyInBinFolder = true)
        {
            LoadCertificates(fileName, "", directlyInBinFolder);
        }

        /// <summary>
        /// Loads certificates.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="password">The password of the certification file.</param>
        /// <param name="directlyInBinFolder">[True] iff the certificate is directly in the .exe folder.</param>
        public void LoadCertificates(string fileName, string password, bool directlyInBinFolder = true)
        {
            string path = !directlyInBinFolder ? fileName : $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\{fileName}";
            X509ClientCertificates = new X509Certificate2Collection { new X509Certificate2(path, password) };
            PublicKey = X509ClientCertificates[0].GetPublicKeyString();
        }

        /// <summary>
        /// Loads the certificate.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="directlyInBinFolder">[True] iff the certificate is directly in the .exe folder.</param>
        public void LoadCertificate(string fileName, bool directlyInBinFolder = true)
        {
            LoadCertificate(fileName, "", directlyInBinFolder);
        }

        /// <summary>
        /// Loads the certificate.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="password">The password of the certification file.</param>
        /// <param name="directlyInBinFolder">[True] iff the certificate is directly in the .exe folder.</param>
        public void LoadCertificate(string fileName, string password, bool directlyInBinFolder = true)
        {
            string path = !directlyInBinFolder ? fileName : $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\{fileName}";
            X509ServerCertificate = new X509Certificate2(path, password);
        }
    }
}