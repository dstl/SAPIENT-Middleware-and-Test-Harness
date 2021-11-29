// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SocketCommsCommon.cs$
// <copyright file="SocketCommsCommon.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices.Communication
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using log4net;

    /// <summary>
    /// class to provide TCP common services
    /// </summary>
    public class SocketCommsCommon
    {
        #region Constant Definitions

        /// <summary>
        /// default maximum packet size in bytes
        /// </summary>
        public const uint MaximumPacketSize = 1024;

        #endregion

        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Public Static Methods

        /// <summary>
        /// Whether connection is OK
        /// </summary>
        /// <param name="client">connection object</param>
        /// <param name="stream">stream object</param>
        /// <returns>true if connected</returns>
        public static bool Connected(TcpClient client, NetworkStream stream)
        {
            bool retval = false;
            if (client != null)
            {
                if ((stream != null) && client.Connected)
                {
                    retval = true;
                }
            }

            return retval;
        }

        /// <summary>
        /// send a data packet to TCP client
        /// </summary>
        /// <param name="msg">message data</param>
        /// <param name="packet_size">size in bytes</param>
        /// <param name="client">connection object</param>
        /// <param name="stream">stream object</param>
        /// <param name="nullTermination">whether to add null on end of message</param>
        /// <returns>true if connection valid</returns>
        public static bool SendPacket(byte[] msg, int packet_size, TcpClient client, NetworkStream stream, bool nullTermination)
        {
            var retval = false;
            if (packet_size > msg.Length)
            {
                Log.Info("Send Failed - Invalid packet size|");
                return true;
            }

            try
            {
                if (Connected(client, stream))
                {
                    // Sending packetSize bytes
                    stream.Write(msg, 0, packet_size);

                    // Null termination of messages
                    if (nullTermination)
                    {
                        stream.WriteByte(0);
                    }

                    retval = true;
                }
                else
                {
                    Log.Info("Send Failed: No Connection");
                }
            }
            catch (IOException ex)
            {
                Log.Info("Send Timeout - connection closed");
            }
            catch (SocketException ex)
            {
                Log.Info("Send Failed - connection closed");
            }
            catch (Exception ex)
            {
                Log.Error("SendPacket error", ex);
            }

            return retval;
        }

        /// <summary>
        /// receive from TCP stream, recoverable in the case of a problem
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="buffer">receive buffer</param>
        /// <param name="offset">offset in to buffer in bytes</param>
        /// <param name="size">size of buffer in bytes</param>
        /// <param name="client">connection object</param>
        /// <returns>true if successful</returns>
        public static bool SingleSafeReceive(NetworkStream stream, byte[] buffer, int offset, ref int size, TcpClient client)
        {
            bool status = Connected(client, stream);

            if (status && (offset + size <= buffer.Length))
            {
                int data_available = DataAvailable(stream);

                if (data_available > 0)
                {
                    int recv_return = stream.Read(buffer, offset, size);

                    switch (recv_return)
                    {
                        case -1:
                        case 0:
                            status = false;
                            size = 0;
                            break;
                        default:
                            size = recv_return;
                            break;
                    }
                }
                else if (data_available < 0)
                {
                    size = 0;
                    status = false;
                }
                else if (!Connected(client, stream))
                {
                    size = 0;
                    status = false;
                }
                else
                {
                    size = 0;
                }
            }

            return status;
        }

        /// <summary>
        /// receive from TCP stream, recoverable in the case of a problem
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="buffer">receive buffer</param>
        /// <param name="offset">offset in to buffer in bytes</param>
        /// <param name="size">size of buffer in bytes</param>
        /// <param name="client">connection object</param>
        /// <returns>true if successful</returns>
        public static bool SafeReceive(NetworkStream stream, byte[] buffer, int offset, int size, TcpClient client)
        {
            int received_bytes = 0;
            bool status = Connected(client, stream);

            while ((received_bytes != size) && status)
            {
                status = Connected(client, stream);
                if (status)
                {
                    int data_available = DataAvailable(stream);

                    if (data_available > 0)
                    {
                        int recv_return = stream.Read(buffer, offset + received_bytes, size - received_bytes);

                        switch (recv_return)
                        {
                            case -1:
                            case 0:
                                status = false;
                                break;
                            default:
                                received_bytes += recv_return;
                                break;
                        }
                    }
                    else if (data_available < 0)
                    {
                        status = false;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Send Null to check whether connection is still alive
        /// </summary>
        /// <param name="client">connection client</param>
        /// <returns>true if still connected</returns>
        public static bool PollConnection(TcpClient client)
        {
            bool retval = true;

            try
            {
                bool blockingState = client.Client.Blocking;
                byte[] buff = new byte[1];
                client.Client.Blocking = false;
                client.Client.Send(buff, 1, 0);
                client.Client.Blocking = blockingState;
            }
            catch (SocketException e)
            {
                // 10035 = WSAEWOULBLOCK
                if (!e.NativeErrorCode.Equals(10035))
                {
                    retval = false;
                }
            }
            catch (ObjectDisposedException oe)
            {
                retval = false;
            }

            return retval;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// whether data available to read
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <returns>1 if there is, 0 if there isn't but connection is ok, -1 if error or connection closed</returns>
        private static int DataAvailable(NetworkStream stream)
        {
            var retval = -1;
            try
            {
                retval = stream.DataAvailable ? 1 : 0;
            }
            catch (Exception ex)
            {
                Log.Error("Data Available Error:" + ex.ToString());
            }

            return retval;
        }

        #endregion
    }
}
