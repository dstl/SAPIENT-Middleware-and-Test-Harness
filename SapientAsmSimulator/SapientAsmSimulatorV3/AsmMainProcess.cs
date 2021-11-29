// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: AsmMainProcess.cs$
// Copyright:         Crown Copyright (c) 2019. See Release/Supply Conditions
// <copyright file="AsmMainProcess.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.IO;
    using System.Threading;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Main Processing class for ASM simulator
    /// </summary>
    public class ASMMainProcess : ISender
    {
        #region Private Data Members

        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// reference to UI interface
        /// </summary>
        private IGUIInterface guiInterface;

        /// <summary>
        /// Socket connection object
        /// </summary>
        private ICommsConnection connection;

        /// <summary>
        /// socket messaging object
        /// </summary>
        private IConnection messenger;

        /// <summary>
        /// thread for looped heartbeat messages
        /// </summary>
        private Thread heartbeatLoopThread;

        /// <summary>
        /// thread for looped detection messages
        /// </summary>
        private Thread detectionLoopThread;

        /// <summary>
        /// thread for looped alert messages
        /// </summary>
        private Thread alertLoopThread;

        /// <summary>
        /// Detection message generator
        /// </summary>
        private DetectionGenerator detectionGenerator;

        /// <summary>
        /// Heartbeat message generator
        /// </summary>
        private HeartbeatGenerator heartbeatGenerator;

        /// <summary>
        /// Alert message generator
        /// </summary>
        private AlertGenerator alertGenerator;

        /// <summary>
        /// Route Plan message generator
        /// </summary>
        private RoutePlanGenerator routePlanGenerator;

        /// <summary>
        /// message logger
        /// </summary>
        private SapientLogger sapientLogger;

        /// <summary>
        /// interval time to use between sending heartbeat messages if looped
        /// </summary>
        private int heartbeatLoopTime;

        /// <summary>
        /// interval time to use between sending detection messages if looped
        /// </summary>
        private int detectionLoopTime;

        #endregion

        /// <summary>
        /// Initializes a new instance of the ASMMainProcess class
        /// </summary>
        /// <param name="guiInterface">link to user interface</param>
        public ASMMainProcess(IGUIInterface guiInterface)
        {
            this.guiInterface = guiInterface;

            string logDirectory = Properties.Settings.Default.LogDirectory;
            logDirectory = string.Format("{0}-ASM{1:D3}", logDirectory, ASMMainProcess.AsmId);

            // if log directory doesn't exist, make it
            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            SetLogPath(logDirectory);

            if (Properties.Settings.Default.Log)
            {
                this.sapientLogger = SapientLogger.CreateLogger(Properties.Settings.Default.LogDirectory, Properties.Settings.Default.LogPrefix, Properties.Settings.Default.IncrementIntervalSeconds);
            }
        }

        /// <summary>
        /// Gets or sets the fixed sensor identifier to use in all messages
        /// </summary>
        public static int AsmId { get; set; }

        /// <summary>
        /// Gets or sets the interval time to use between sending heartbeat messages if looped
        /// </summary>
        public int HeartbeatLoopTime
        {
            get
            {
                return this.heartbeatLoopTime;
            }

            set
            {
                if (value > 0)
                {
                    this.heartbeatLoopTime = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval time to use between sending detection messages if looped
        /// </summary>
        public int DetectionLoopTime
        {
            get
            {
                return this.detectionLoopTime;
            }

            set
            {
                if (value > 0)
                {
                    this.detectionLoopTime = value;
                }
            }
        }

        /// <summary>
        /// Initialize main processing
        /// </summary>
        public void Initialise()
        {
            var serverName = Properties.Settings.Default.SapientDataAgentAddress;
            int serverPort = Properties.Settings.Default.SapientDataAgentPort;

            serverPort += ASMMainProcess.AsmId;

            var client = new SapientClient(serverName, serverPort);

            this.connection = client;
            this.messenger = client;
            this.messenger.SetNoDelay(true);
            this.messenger.SetSendNullTermination(Properties.Settings.Default.sendNullTermination);

            // if only sending data, currently the SapientComms Librray won't reconnect
            // This isn't a problem as we need bi-directional comms
            const bool SEND_ONLY_CONNECTION = false;
            this.connection.Start(1024 * 1024, SEND_ONLY_CONNECTION);
            this.connection.SetDataReceivedCallback(this.DataCallback);
            this.detectionGenerator = new DetectionGenerator();
            this.heartbeatGenerator = new HeartbeatGenerator();
            TaskMessage.HeartbeatGenerator = this.heartbeatGenerator;
            this.alertGenerator = new AlertGenerator();
            this.detectionGenerator.ChangeObjectID = Properties.Settings.Default.IndividualDetectionIDs;
            this.detectionGenerator.ImageURL = Properties.Settings.Default.DetectionImageURL;
            this.detectionGenerator.TypeString = Properties.Settings.Default.DetectionType;

            this.routePlanGenerator = new RoutePlanGenerator();
            this.routePlanGenerator.ChangeRoutePlanID = Properties.Settings.Default.IndividualDetectionIDs;

            this.detectionLoopTime = 100;
            this.heartbeatLoopTime = 5;
            MessageSender.FragmentData = Properties.Settings.Default.PacketFragmentDelay > 0;
            MessageSender.PacketDelay = Properties.Settings.Default.PacketFragmentDelay;
        }

        /// <summary>
        /// Shut down main processing
        /// </summary>
        public void Shutdown()
        {
            this.connection.Shutdown();
        }

        /// <summary>
        /// Return number of detection messages sent
        /// </summary>
        /// <returns>detection count</returns>
        public long DetectionCount()
        {
            long retval = 0;
            if (this.detectionGenerator != null)
            {
                retval = this.detectionGenerator.MessageCount;
            }

            return retval;
        }

        /// <summary>
        /// Return number of heartbeat messages sent
        /// </summary>
        /// <returns>heartbeat count</returns>
        public long HeartBeatCount()
        {
            long retval = 0;
            if (this.heartbeatGenerator != null)
            {
                retval = this.heartbeatGenerator.MessageCount;
            }

            return retval;
        }

        /// <summary>
        /// Return number of alert messages sent
        /// </summary>
        /// <returns>alert count</returns>
        public long AlertCount()
        {
            long retval = 0;
            if (this.alertGenerator != null)
            {
                retval = this.alertGenerator.MessageCount;
            }

            return retval;
        }

        /// <summary>
        /// Send single registration message
        /// </summary>
        public void SendRegistration()
        {
            RegistrationGenerator.GenerateRegistration(this.messenger, this.guiInterface, this.sapientLogger);
        }

        /// <summary>
        /// Send detection message once or in a loop
        /// </summary>
        public void SendDetectionLoop()
        {
            if (this.detectionLoopThread == null || this.detectionLoopThread.IsAlive == false)
            {
                this.detectionGenerator.LoopTime = this.DetectionLoopTime;
                this.detectionLoopThread = new Thread(o => this.detectionGenerator.GenerateDetections(o, this.sapientLogger));
                this.detectionLoopThread.Start(this.messenger);
            }
        }

        /// <summary>
        /// Send heartbeat message once or in a loop
        /// </summary>
        public void SendHeartbeatLoop()
        {
            if (this.heartbeatLoopThread == null || this.heartbeatLoopThread.IsAlive == false)
            {
                this.heartbeatGenerator.LoopTime = this.HeartbeatLoopTime * 1000;
                this.heartbeatLoopThread = new Thread(o => this.heartbeatGenerator.GenerateStatus(o, this.sapientLogger));
                this.heartbeatLoopThread.Start(this.messenger);
            }
        }

        /// <summary>
        /// Send alert message once or in a loop
        /// </summary>
        public void SendAlertLoop()
        {
            if (this.alertLoopThread == null || this.alertLoopThread.IsAlive == false)
            {
                this.alertGenerator.LoopTime = this.DetectionLoopTime;
                this.alertLoopThread = new Thread(o => this.alertGenerator.GenerateAlert(o, this.sapientLogger));
                this.alertLoopThread.Start(this.messenger);
            }
        }

        /// <summary>
        /// Toggle whether to loop sending detection messages
        /// </summary>
        /// <param name="loop">true if looping</param>
        public void SetDetectionLoopState(bool loop)
        {
            this.detectionGenerator.LoopMessages = loop;
            if (!loop)
            {
                try
                {
                    this.detectionLoopThread.Abort();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Toggle whether to loop sending heartbeat messages
        /// </summary>
        /// <param name="loop">true if looping</param>
        public void SetHeartbeatLoopState(bool loop)
        {
            this.heartbeatGenerator.LoopMessages = loop;
            if (!loop)
            {
                try
                {
                    this.heartbeatLoopThread.Abort();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Toggle whether to loop sending alert messages
        /// </summary>
        /// <param name="loop">true if looping</param>
        public void SetAlertLoopState(bool loop)
        {
            this.alertGenerator.LoopMessages = loop;
            if (!loop)
            {
                try
                {
                    this.alertLoopThread.Abort();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Method to read xml from file.
        /// Checking could be done here to make sure XML is correct 
        /// However for testing this is not being done
        /// </summary>
        /// <param name="input_filename">Path to XML file to be loaded.</param>
        public void ReadAndSendFile(string input_filename)
        {
            using (var sr = new StreamReader(input_filename))
            {
                var whole = sr.ReadToEnd();
                var genreal = System.Text.Encoding.UTF8.GetBytes(whole);
                this.messenger.SendMessage(genreal, genreal.Length);
                this.guiInterface.UpdateOutputWindow("Sent " + Path.GetFileName(input_filename));
            }
        }


        public void AddDataReceivedCallback(SapientCommsCommon.DataReceivedCallback callback)
        {
            this.connection.SetDataReceivedCallback(callback);
        }

        /// <summary>
        /// Send route plan message once or in a loop
        /// </summary>
        public void SendRoutePlan()
        {
            this.routePlanGenerator.GenerateRoutePlans(this.messenger, this.sapientLogger);
        }

        /// <summary>
        /// Set associated filename for detections and alerts
        /// </summary>
        /// <param name="filename">filename string</param>
        public void SetAssociatedFilename(string filename)
        {
            if(this.detectionGenerator!=null)
            {
                this.detectionGenerator.ImageURL = filename;
            }

            AlertGenerator.ImageURL = filename;
        }

        /// <summary>
        /// callback method for communication 
        /// </summary>
        /// <param name="msg_buffer">message buffer</param>
        /// <param name="size">message size</param>
        /// <param name="client">client connection</param>
        private void DataCallback(ref byte[] msg_buffer, int size, IConnection client)
        {
            var linestring = System.Text.Encoding.UTF8.GetString(msg_buffer, 0, size);
            this.ProcessIncomingMessage(linestring);
        }

        /// <summary>
        /// function to process the message coming in
        /// </summary>
        /// <param name="message">the xml string from the client</param>
        private void ProcessIncomingMessage(string message)
        {
            message = message.TrimStart('\r', '\n', '\0');
            if (message.Contains("</SensorRegistrationACK>"))
            {
                try
                {
                    Log.Info("SensorRegistrationACK Received");
                    var id = (SensorRegistrationACK)ConfigXMLParser.Deserialize(typeof(SensorRegistrationACK), message);
                    ASMMainProcess.AsmId = id.sensorID;

                    this.guiInterface.UpdateASMText(ASMMainProcess.AsmId.ToString());

                    // Assign asm ID to all generator classes
                    XmlGenerators.ASMId = ASMMainProcess.AsmId;
                    TaskMessage.AsmId = ASMMainProcess.AsmId;
                    RegistrationGenerator.AsmId = ASMMainProcess.AsmId;

                    this.guiInterface.UpdateOutputWindow("SensorRegistrationACK Received:\r\nASM ID: " + ASMMainProcess.AsmId + " Latency(ms): " + +(DateTime.UtcNow - RegistrationGenerator.SendRegistrationTime).TotalMilliseconds);
                    Log.Info("ASM ID: " + ASMMainProcess.AsmId.ToString() + " Latency(ms): " + (DateTime.UtcNow - RegistrationGenerator.SendRegistrationTime).TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Registration ACK Failed " + ex);
                }
            }
            else if (message.Contains("</SensorTask>"))
            {
                try
                {
                    Log.Info("Task Message Received");
                    TaskMessage.ProcessSensorTaskMessage(message, this.messenger, this.guiInterface, this.sapientLogger);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Task Message Failed " + ex);
                }
            }
            else if (message.Contains("</AlertResponse>"))
            {
                try
                {
                    Log.Info("AlertResponse Message Received");
                    var response = (AlertResponse)ConfigXMLParser.Deserialize(typeof(AlertResponse), message);
                    this.guiInterface.UpdateOutputWindow("AlertResponse Received alertID: " + response.alertID.ToString());
                    Log.Info("AlertResponse alertID:" + response.alertID);
                    Log.Info(message);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Task Message Failed " + ex);
                }
            }
            else if (message.Contains("</Error>"))
            {
                try
                {
                    var e = (Error)ConfigXMLParser.Deserialize(typeof(Error), message);
                    Log.Error("Error Received\n" + e.errorMessage);
                    this.guiInterface.UpdateOutputWindow("Error Received:\r\n" + e.errorMessage);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Error Message Failed " + ex);
                }
            }
        }

        /// <summary>
        /// set log path for log4net
        /// </summary>
        /// <param name="logDirectory">folder to log to</param>
        private static void SetLogPath(string logDirectory)
        {
            log4net.Config.XmlConfigurator.Configure();
            log4net.Repository.Hierarchy.Hierarchy h =
            (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            foreach (log4net.Appender.IAppender a in h.Root.Appenders)
            {
                if (a is log4net.Appender.FileAppender)
                {
                    log4net.Appender.FileAppender fa = (log4net.Appender.FileAppender)a;

                    // Programmatically set this to the desired location here
                    string logFileLocation = System.IO.Path.Combine(logDirectory, "asmlog.txt");

                    fa.File = logFileLocation;
                    fa.ActivateOptions();
                    break;
                }
            }
        }
    }
}
