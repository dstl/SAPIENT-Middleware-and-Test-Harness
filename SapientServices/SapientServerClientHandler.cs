// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientServerClientHandler.cs$
// <copyright file="SapientServerClientHandler.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices.Communication
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using log4net;
    using System.Text;

    /// <summary>
    /// class to Handle Sapient Server Client Connections
    /// </summary>
    public class SapientServerClientHandler : IConnection
    {
        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(typeof(SapientServerClientHandler));

        private static readonly ILog SendLog = LogManager.GetLogger("SendLogger");

        private readonly TcpClient client;

        private readonly NetworkStream stream;

        /// <summary>
        /// Lock object
        /// </summary>
        private object thisLock = new object();

        private bool thread_running;

        private readonly Thread client_receive_thread;

        private SapientCommsCommon.DataReceivedCallback data_receivedcallback;

        private SapientCommsCommon.StatusCallback connect_callback;

        private readonly uint max_packet_size;

        private bool sendNullTermination;

        private bool dataReceived;

        private bool dataSent;

        private bool connected;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client_socket"></param>
        /// <param name="maximum_packet_size"></param>
        /// <param name="send_only"></param>
        /// <param name="connection_id"></param>
        public SapientServerClientHandler(TcpClient client_socket, uint maximum_packet_size, bool send_only, uint connection_id)
        {
            client = client_socket;

            // Get a stream object for reading and writing
            stream = client.GetStream();

            max_packet_size = maximum_packet_size;

            if (!send_only)
            {
                client_receive_thread = new Thread(ClientReceiveThread) { Name = "Server Receive Thread" };
                client_receive_thread.Start();
            }

            ConnectionID = connection_id;

            // get end point for information
            if (client != null)
            {
                RemoteEndPoint = client.Client.RemoteEndPoint;
            }
        }

        #endregion

        #region Properties

        // Connection Properties
        public uint ConnectionID { get; private set; }

        public EndPoint RemoteEndPoint { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set Data received callback method
        /// </summary>
        /// <param name="callback"></param>
        public void SetDataReceivedCallback(SapientCommsCommon.DataReceivedCallback callback)
        {
            data_receivedcallback += callback;
        }

        /// <summary>
        /// Set connection connected callback method
        /// </summary>
        /// <param name="statuscallback"></param>
        public void SetConnectedCallback(SapientCommsCommon.StatusCallback statuscallback)
        {
            connect_callback += statuscallback;
        }

        /// <summary>
        /// set no delay flag
        /// </summary>
        /// <param name="no_delay"></param>
        public void SetNoDelay(bool no_delay)
        {
            if (client != null)
            {
                client.NoDelay = no_delay;
            }
        }

        /// <summary>
        /// set whether to end subsequent messages with null termination
        /// </summary>
        /// <param name="useNullTermination">whether to include null termination on sent messages</param>
        public void SetSendNullTermination(bool useNullTermination)
        {
          sendNullTermination = useNullTermination;
        }

        /// <summary>
        /// Poll whether we still have a connection
        /// </summary>
        /// <returns>is connected</returns>
        public bool IsConnected()
        {
          if (connected && (client != null))
          {
            connected = SocketCommsCommon.Connected(client, stream);

            if (connected && !dataReceived && !dataSent)
            {
              connected = SocketCommsCommon.PollConnection(client);

              if (!connected)
              {
                logger.Error("Client Connection " + this.ConnectionID + " lost");
              }
            }

            lock (thisLock)
            {
              dataSent = false;
              dataReceived = false;
            }
          }

          return connected;
        }

        /// <summary>
        /// close connection with client and stop thread
        /// </summary>
        public void Shutdown()
        {
            connected = false;
            thread_running = false;
            if (client_receive_thread != null)
            {
                if (client_receive_thread.IsAlive)
                {
                    client_receive_thread.Join(1000);
                }
            }

            if (connect_callback != null)
            {
              connect_callback("Socket Disconnected", this);
            }

            data_receivedcallback = null;
            connect_callback = null;
            client.Close();
        }

        /// <summary>
        /// send a data message to TCP client
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msg_size"></param>
        /// <returns></returns>
        public bool SendMessage(byte[] msg, int msg_size)
        {
            logger.DebugFormat("SendPacket: {0} bytes", msg_size);            
            bool retval = SocketCommsCommon.SendPacket(msg, msg_size, client, stream, this.sendNullTermination);
            lock (thisLock)
            {
              dataSent = retval;
            }
            SendLog.InfoFormat("Sending message to {0}, Message contents: {1}", client.Client.RemoteEndPoint, Encoding.UTF8.GetString(msg));

            return retval;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Receive thread from TCP client connection
        /// </summary>
        private void ClientReceiveThread()
        {
            try
            {
                thread_running = true;
                connected = true;
                byte[] bytes = new byte[max_packet_size];

                // Loop to receive all the data sent by the client.
                bool retval = true;

                do
                {
                    retval = SocketCommsCommon.SafeReceive(stream, bytes, 0, 1, client);

                    if (thread_running && retval)
                    {
                        // start stop watch for timing diagnostics
                        Stopwatch stopWatch = new Stopwatch();
                        double usecPerTick = (1000 * 1000) / (double)Stopwatch.Frequency;
                        stopWatch.Start();

                        var size = (int)max_packet_size - 1;
                        retval = SocketCommsCommon.SingleSafeReceive(stream, bytes, 1, ref size, client);

                        double queryElapsedusec = stopWatch.ElapsedTicks * usecPerTick;
                        stopWatch.Stop();
                        logger.Debug("srx us:" + queryElapsedusec.ToString("F1"));

                        lock (thisLock)
                        {
                            dataReceived = true;
                        }

                        if (thread_running && retval)
                        {
                            size++; // add initially read byte to size

                            // if any callback functions defined pass them the data
                            if (data_receivedcallback != null)
                            {
                                data_receivedcallback(ref bytes, size, this);
                            }
                        }
                    }
                } while (thread_running && retval);

                connected = false;

                // close stream
                stream.Close();

                // Shutdown and end connection
                client.Close();
            }
            catch (SocketException ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Socket Disconnected");
                if (connect_callback != null)
                {
                    connect_callback("Socket Disconnected", this);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        #endregion
    }
}
