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
using Network.Enums;
using System.Net.Sockets;
using System.Threading;

namespace Network.Secure
{
    public abstract class TcpSecureConnection : SecureConnection
    {
        internal TcpSecureConnection(TcpClient tcpClient, SecureConnectionConfiguration configuration)
            : base(tcpClient, configuration)
        {

        }

        protected override byte[] ReadBytes(int amount)
        {
            if (amount == 0) return new byte[0];
            byte[] requestedBytes = new byte[amount];
            int receivedIndex = 0;
            int currentByte = -1;
            while (receivedIndex < amount)
            {
                while ((currentByte = SslStream.ReadByte()) == -1)
                    Thread.Sleep(IntPerformance);

                requestedBytes[receivedIndex] = (byte)currentByte;
                receivedIndex++;
            }

            return requestedBytes;
        }

        protected override void WriteBytes(byte[] bytes)
        {
            SslStream.Write(bytes);
        }

        protected override void HandleUnknownPacket()
        {
            Logger.Log($"Connection can't handle the received packet. No listener defined.", LogLevel.Error);
            CloseHandler(CloseReason.ReadPacketThreadException);
        }
    }
}
