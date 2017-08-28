#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : Network
// Author           : Thomas Christof
// Created          : 02-03-2016
//
// Last Modified By : Thomas Christof
// Last Modified On : 28.05.2016
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
#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Network.Enums;
using InTheHand.Net.Sockets;
using System.Threading.Tasks;

namespace Network.Secure
{
    public class SecureServerConnectionContainer : ConnectionContainer
    {
        private TcpListener tcpListener;
        private event Action<Connection, ConnectionType> connectionEstablished;
        private event Action<Connection, ConnectionType, CloseReason> connectionLost;
        private ConcurrentBag<TcpSecureConnection> tcpSecureConnections = new ConcurrentBag<TcpSecureConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnectionContainer" /> class.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="start">if set to <c>true</c> then the instance automatically starts to listen to tcp/udp/bluetooth clients.</param>
        internal SecureServerConnectionContainer(string ipAddress, int port, bool start = true)
                : base(ipAddress, port)
            {
            if (start)
                Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnectionContainer" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="start">if set to <c>true</c> then the instance automatically starts to listen to clients.</param>
        internal SecureServerConnectionContainer(int port, bool start = true)
                : this(System.Net.IPAddress.Any.ToString(), port, start)
        {

        }

        /// <summary>
        /// Gets a value indicating whether the tcp server is online or not.
        /// </summary>
        /// <value><c>true</c> if this instance is online; otherwise, <c>false</c>.</value>
        public bool IsOnline { get; private set; } = false;

        /// <summary>
        /// Gets all the connected TCP connections.
        /// </summary>
        /// <value>The tc p_ connections.</value>
        public List<TcpSecureConnection> TCP_SECURE_Connection { get { return tcpSecureConnections.ToList(); } }

        /// <summary>
        /// Gets the connection count. (Clients)
        /// </summary>
        public int Count { get { return tcpSecureConnections.Count; } }

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        public SecureConnectionConfiguration SecureConnectionConfiguration { get; internal set; }

        /// <summary>
        /// Occurs when [connection closed]. This action will be called if a TCP or an UDP has been closed.
        /// If a TCP connection has been closed, all its attached UDP connections are lost as well.
        /// If a UDP connection has been closed, the attached TCP connection may still be alive.
        /// </summary>
        public event Action<Connection, ConnectionType, CloseReason> ConnectionLost
        {
            add { connectionLost += value; }
            remove { connectionLost -= value; }
        }

        /// <summary>
        /// Occurs when a TCP or an UDP connection has been established.
        /// </summary>
        public event Action<Connection, ConnectionType> ConnectionEstablished
        {
            add { connectionEstablished += value; }
            remove { connectionEstablished -= value; }
        }

        /// <summary>
        /// Starts to listen to the given port and ipAddress.
        /// </summary>
        public async void Start()
        {
            if (IsOnline) return;

            tcpListener = new TcpListener(System.Net.IPAddress.Parse(IPAddress), Port);
            IsOnline = !IsOnline;
            tcpListener.Start();

            while (IsOnline)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ConnectionResult connectionResult = ConnectionResult.CertificateError;
                var securityConfiguration = SecureConnectionConfiguration;
                TcpSecureConnection tcpConnection = ConnectionFactory.CreateSecureConnectionServer(ref securityConfiguration, tcpClient, out connectionResult);

                if (connectionResult != ConnectionResult.Connected)
                    continue;

                tcpConnection.ConnectionClosed += connectionClosed;
                tcpSecureConnections.Add(tcpConnection);

                //Inform all subscribers.
                if (connectionEstablished != null &&
                    connectionEstablished.GetInvocationList().Length > 0)
                    connectionEstablished(tcpConnection, ConnectionType.TCP);

                KnownTypes.ForEach(tcpConnection.AddExternalPackets);
                //Now that the server registered all the methods, unlock the client.
                tcpConnection.UnlockRemoteConnection();
            }
        }

        /// <summary>
        /// TCPs the or UDP connection closed.
        /// </summary>
        /// <param name="closeReason">The close reason.</param>
        /// <param name="connection">The connection.</param>
        private void connectionClosed(CloseReason closeReason, Connection connection)
        {
            TcpSecureConnection tcpConnection = (TcpSecureConnection)connection;
            tcpSecureConnections = new ConcurrentBag<TcpSecureConnection>(tcpSecureConnections.Except(new[] { tcpConnection }));

            if (connectionLost != null &&
                connectionLost.GetInvocationList().Length > 0)
                connectionLost(connection, ConnectionType.TCP, closeReason);
        }

        /// <summary>
        /// Stops listening to tcp ssl clients.
        /// </summary>
        public void Stop()
        {
            if (IsOnline) tcpListener.Stop();
            IsOnline = !IsOnline;
        }

        /// <summary>
        /// Closes all the tcp and udp connections.
        /// </summary>
        public void CloseConnections(CloseReason reason)
        {
            tcpSecureConnections.ToList().ForEach(c => c.Close(reason));
            //Clear or reassign the connection containers.
            tcpSecureConnections = new ConcurrentBag<TcpSecureConnection>();
        }

        /// <summary>
        /// Sends a broadcast to all the connected tcp connections.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void BroadCast(Packet packet)
        {
            tcpSecureConnections.ToList().ForEach(c => c.Send(packet));
        }

        public override string ToString()
        {
            return $"ServerConnectionContainer. IsOnline {IsOnline}.";
        }
    }
}