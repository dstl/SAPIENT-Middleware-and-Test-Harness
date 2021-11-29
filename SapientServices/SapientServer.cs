// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientServer.cs$
// <copyright file="SapientServer.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices.Communication
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using log4net;

    /// <summary>
    /// class to provide Sapient Server Comms Connections
    /// </summary>
    public class SapientServer : ICommsConnection
    {
        #region Private Static Data Members

        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(typeof(SapientServer));

        #endregion

        #region Private Data Members

        private TcpListener server;

        private bool thread_running;

        private bool send_only;

        private bool no_delay;

        private bool sendNullTermination;

        private Thread listener_thread;

        private readonly string local_address;

        private readonly int port = 12000;

        private readonly List<SapientServerClientHandler> connections = new List<SapientServerClientHandler>();

        private SapientCommsCommon.DataReceivedCallback data_receivedcallback;
        private SapientCommsCommon.StatusCallback connect_callback;

        private uint max_packet_size = SocketCommsCommon.MaximumPacketSize;

        private readonly object this_lock = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="local_server_address"></param>
        /// <param name="serverport"></param>
        public SapientServer(string local_server_address, int serverport)
        {
            local_address = local_server_address;
            port = serverport;
            ConnectionID = 1;
        }

        #endregion

        #region Properties

        // Connection Properties
        public uint ConnectionID { get; private set; }

        // Number of Clients connected
        public int NumClients { get; private set; }

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
        /// <param name="_no_delay"></param>
        public void SetNoDelay(bool _no_delay)
        {
            no_delay = _no_delay;
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
        /// Start comms 
        /// </summary>
        /// <param name="maximum_packet_size"></param>
        /// <param name="_send_only"></param>
        public void Start(uint maximum_packet_size, bool _send_only)
        {
            send_only = _send_only;
            max_packet_size = maximum_packet_size;
            Start();
        }

        /// <summary>
        /// Start server 
        /// </summary>
        public void Start()
        {
            listener_thread = new Thread(Listen) { Name = port + " Listener" };
            listener_thread.Start();
        }

        /// <summary>
        /// close listener connection and stop server threads
        /// </summary>
        public void Shutdown()
        {
            thread_running = false;

            // Stop listening for new clients.
            if (server != null)
            {
                server.Stop();
            }

            if (listener_thread != null)
            {
                if (listener_thread.IsAlive)
                {
                    listener_thread.Join(1000);
                }
            }

            // shutdown client connections
            lock (this_lock)
            {
                foreach (SapientServerClientHandler connection in connections)
                {
                    connection.Shutdown();
                }
            }

            data_receivedcallback = null;
            connect_callback = null;
        }

        /// <summary>
        /// Poll whether connections are still alive
        /// </summary>
        /// <returns>true</returns>
        public bool IsConnected()
        {
          logger.DebugFormat("{1}:ISConnected: {0} connections", connections.Count, port);

          lock (this_lock)
          {
            int i = 0;

            // send to all connected client connections
            while (i < connections.Count)
            {
              if (!connections[i].IsConnected())
              {
                // remove any
                connections[i].Shutdown();
                connections.RemoveAt(i);
              }
              else
              {
                i++;
              }
            }
          }

          NumClients = connections.Count;
          return NumClients > 0;
        }

        /// <summary>
        /// send a data packet to a specific client
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msg_size"></param>
        /// <param name="connection_id"></param>
        /// <returns></returns>
        public bool SendMessage(byte[] msg, int msg_size, uint connection_id)
        {
            lock (this_lock)
            {
                var sent = false;
                var i = 0;
                var connection_found = false;

                // send to all specified client connection
                while ((i < connections.Count) && !connection_found)
                {
                    if (connections[i].ConnectionID == connection_id)
                    {
                        //// logger.Debug("Sending packet to client.");
                        sent = connections[i].SendMessage(msg, msg_size);
                        connection_found = true;
                    }

                    ++i;
                }

                if (!connection_found)
                {
                    logger.DebugFormat("Connection {0} not found to send message to", connection_id);
                }

                return sent;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Server Listener thread
        /// </summary>
        private void Listen()
        {
            try
            {
                // Set the TcpListener.
                IPAddress local_addr = IPAddress.Parse(local_address);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(local_addr, port);

                // Start listening for client requests.
                server.Start();

                thread_running = true;

                // Enter the listening loop. 
                while (thread_running)
                {
                    logger.Info("Waiting for a connection... ");

                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    var client = server.AcceptTcpClient();
                    client.ReceiveBufferSize = (int)max_packet_size;
                    client.SendBufferSize = (int)max_packet_size;
                    client.NoDelay = no_delay;
                    logger.Info("Connection " + ConnectionID.ToString("D") + " Connected");

                    // new client connection
                    var client_handler = new SapientServerClientHandler(client, max_packet_size, send_only, ConnectionID);

                    client_handler.SetDataReceivedCallback(data_receivedcallback);
                    client_handler.SetConnectedCallback(connect_callback);
                    client_handler.SetSendNullTermination(this.sendNullTermination);

                    // add callback functions
                    connections.Add(client_handler);
                    NumClients = connections.Count;

                    if (connect_callback != null)
                    {
                        connect_callback("Connected", client_handler);
                    }

                    ConnectionID++;
                }
            }
            catch (SocketException e)
            {
                if (thread_running)
                {
                    logger.ErrorFormat("SocketException: {0}", e);
                }
                else
                {
                    logger.Error(port + "Server Closed");
                }
            }
        }
        #endregion
    }
}
