// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientClient.cs$
// <copyright file="SapientClient.cs" company="QinetiQ">
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

    /// <summary>
    /// class to provide TCP Client services to Sapient
    /// </summary>
    public class SapientClient : ICommsConnection, IConnection
    {
        #region Private Static Data Members

        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(typeof(SapientClient));

        #endregion

        #region Private Data Members

        /// <summary>
        /// server IP / name for this connection
        /// </summary>
        private readonly string server_name = "server";

        /// <summary>
        /// TCP port for this connection
        /// </summary>
        private readonly int server_port = 12000;

        private TcpClient client;

        // Get a stream object for reading and writing
        private NetworkStream stream;

        /// <summary>
        /// Lock object
        /// </summary>
        private object thisLock = new object();

        private bool thread_running;

        private bool send_only;

        private bool no_delay;

        private bool sendNullTermination;

        private bool dataReceived;

        private bool dataSent;

        private bool connected;

        private Thread client_receive_thread;

        private SapientCommsCommon.DataReceivedCallback data_receivedcallback;
        private SapientCommsCommon.StatusCallback connect_callback;

        private uint max_packet_size = SocketCommsCommon.MaximumPacketSize;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientClient" /> class.
        /// </summary>
        /// <param name="server">server IP / name for this connection</param>
        /// <param name="port">TCP port for this connection</param>
        public SapientClient(string server, int port)
        {
            server_name = server;
            server_port = port;
            ConnectionID = 1;
            ConnectionName = "Server";
        }

        #endregion

        #region Properties

        // Connection Properties
        public uint ConnectionID { get; private set; }

        public EndPoint RemoteEndPoint { get; private set; }

        public string ConnectionName { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set callback method to call when data is received
        /// </summary>
        /// <param name="callback">callback method to call</param>
        public void SetDataReceivedCallback(SapientCommsCommon.DataReceivedCallback callback)
        {
            data_receivedcallback += callback;
        }

        /// <summary>
        /// Set callback method to call when connected or disconnected
        /// </summary>
        /// <param name="statuscallback">callback method to call</param>
        public void SetConnectedCallback(SapientCommsCommon.StatusCallback statuscallback)
        {
            connect_callback += statuscallback;
        }

        /// <summary>
        /// set no delay flag
        /// </summary>
        /// <param name="_no_delay">set client no delay</param>
        public void SetNoDelay(bool _no_delay)
        {
            no_delay = _no_delay;
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
                logger.ErrorFormat("Connection to {0} lost", ConnectionName);
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
        /// Start comms
        /// </summary>
        /// <param name="maximum_packet_size">maximum packet size in bytes</param>
        /// <param name="_send_only">send only connection</param>
        public void Start(uint maximum_packet_size, bool _send_only)
        {
            send_only = _send_only;
            max_packet_size = maximum_packet_size;
            Start();
        }

        /// <summary>
        /// Start comms
        /// </summary>
        public void Start()
        {
            if (!send_only)
            {
                client_receive_thread = new Thread(ClientReceiveThread)
                                            {
                                                Name = server_name + ":" + server_port + " Client Receive Thread",
                                            };
                client_receive_thread.Start();
            }
            else
            {
                ConnectToServer();
            }
        }

        /// <summary>
        /// shutdown communication with server and stop thread
        /// </summary>
        public void Shutdown()
        {
            thread_running = false;
            Close(); // close the socket
            if (client_receive_thread != null)
            {
                if (client_receive_thread.IsAlive)
                {
                    client_receive_thread.Join(1000);
                }
            }
        }

        /// <summary>
        /// send a data message to TCP client
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="msg_size">size in bytes</param>
        /// <returns>if successful</returns>
        public bool SendMessage(byte[] msg, int msg_size)
        {
            logger.DebugFormat("SendPacket: {0} bytes", msg_size);
            bool retval = SocketCommsCommon.SendPacket(msg, msg_size, client, stream, this.sendNullTermination);
            lock (thisLock)
            {
              dataSent = retval;
            }

            return retval;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Receive thread from TCP client connection
        /// </summary>
        private void ClientReceiveThread()
        {
            thread_running = true;

            while (thread_running)
            {
                try
                {
                    var bytes = new byte[max_packet_size];

                    ConnectToServer();

                    // Loop to receive all the data sent by the client.
                    while (thread_running && SocketCommsCommon.SafeReceive(stream, bytes, 0, 1, client))
                    {
                        // start stop watch for timing diagnostics
                        Stopwatch stopWatch = new Stopwatch();
                        double usecPerTick = (1000 * 1000) / (double)Stopwatch.Frequency;
                        stopWatch.Start();

                        int size = (int)max_packet_size - 1;
                        SocketCommsCommon.SingleSafeReceive(stream, bytes, 1, ref size, client);

                        size++; // add initially read byte to size

                        double queryElapsedusec = stopWatch.ElapsedTicks * usecPerTick;

                        logger.Debug("crx us:" + queryElapsedusec.ToString("F1"));

                        lock (thisLock)
                        {
                            dataReceived = true;
                        }

                        // if any callback functions defined pass them the data
                        if (data_receivedcallback != null)
                        {
                            data_receivedcallback(ref bytes, size, this);
                        }
                    }


                    Close();
                }
                catch (SocketException ex)
                {
                    logger.Error(ex.ToString());
                    logger.ErrorFormat("Socket Disconnected to {0}", ConnectionName);
                    if (connect_callback != null)
                    {
                        connect_callback("Socket Disconnected", this);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }

                if (thread_running)
                {
                    Thread.Sleep(1000); // wait a second before attempting to reconnect
                }
            } // end of outer while loop
        }

        /// <summary>
        /// method to close the socket
        /// </summary>
        private void Close()
        {
            connected = false;
            if (stream != null)
            {
                // close stream
                stream.Close();
            }

            if (client != null)
            {
              if (client.Client.Connected)
              {
                logger.InfoFormat("Socket Connection to {0} Closed", ConnectionName);
              }

              // close socket
              client.Close();
            }
        }

        /// <summary>
        /// create socket connection to camera server
        /// </summary>
        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient(server_name, server_port)
                             {
                                 NoDelay = no_delay,
                                 ReceiveTimeout = 10000,
                                 ReceiveBufferSize = (int)max_packet_size,
                                 SendBufferSize = (int)max_packet_size,
                             };
                stream = client.GetStream();
                if (stream != null)
                {
                    RemoteEndPoint = client.Client.RemoteEndPoint;

                    logger.InfoFormat("Connected To {0}", ConnectionName);
                    if (connect_callback != null)
                    {
                        connect_callback("Connected To Server", this);
                    }

                    connected = true;
                }
            }
            catch (SocketException ex)
            {
                logger.Error("Unable to connect to " + ConnectionName + " on host:" + server_name + " Port:" + server_port);

                if (ex.ErrorCode == (int)SocketError.ConnectionRefused)
                {
                    logger.ErrorFormat("Cannot connect to {2} on Host:{0} Port:{1}. Check these settings and then check that the {2} is installed and running.", server_name, server_port, ConnectionName);
                }
                else
                {
                    logger.Error(ex);
                }

                if (connect_callback != null)
                {

                    connect_callback("Unable to connect to server", this);
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
