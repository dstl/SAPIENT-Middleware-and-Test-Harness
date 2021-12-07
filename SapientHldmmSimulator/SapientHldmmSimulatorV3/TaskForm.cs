// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskForm.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// task form window class
    /// </summary>
    public partial class TaskForm : Form
    {
        private IConnection messenger;
        private SapientLogger sapient_logger;

        private static double start_longitude = Properties.Settings.Default.startLongitude;
        private static double start_latitude = Properties.Settings.Default.startLatitude;
        private double maxLoopLatitude = Properties.Settings.Default.maxLatitude;
        private double latitude = start_latitude;
        private double longitude = start_longitude;

        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ICommsConnection connection;
        private DetectionGenerator detectionGenerator;
        private AlertGenerator alertGenerator;
        private TaskGenerator taskGenerator;
        private LookAtTaskGenerator lookAtTaskGenerator;
        private XYZLookAtTaskGenerator xyzLookAtTaskGenerator;
        private ObjectiveGenerator objectiveGenerator;
        private RegionTaskGenerator regionTaskGenerator;

    
        private Thread background_detections;
        private Thread background_tasks;
        /// <summary>
        /// form constructor
        /// </summary>
        public TaskForm()
        {
            PTZForm.Randomize();

            InitializeComponent();
            open_file_dialog.InitialDirectory = Application.StartupPath;
            open_file_dialog.Filter = "Xml Files (.xml)|*.xml|All Files (*.*)|*.*";
            open_file_dialog.FilterIndex = 1;
            open_file_dialog.FileName = "*.xml";

            openLogFile.InitialDirectory = Application.StartupPath;
            openLogFile.Filter = "Log Files (.log)|*.log|All Files (*.*)|*.*";
            openLogFile.FilterIndex = 1;
            openLogFile.FileName = "*.log";

            TaskGenerator.SendToHldmm = Properties.Settings.Default.sendToHLDMM;

            this.detectionGenerator = new DetectionGenerator();
            this.alertGenerator = new AlertGenerator();
            this.taskGenerator = new TaskGenerator();
            this.lookAtTaskGenerator = new LookAtTaskGenerator();
            this.xyzLookAtTaskGenerator = new XYZLookAtTaskGenerator();
            this.objectiveGenerator = new ObjectiveGenerator();
            this.regionTaskGenerator = new RegionTaskGenerator();
            SetAssociatedFilename(filenameTextBox.Text);
        }

        /// <summary>
        /// GUI load method. sets up the connection and waits for traffic. 
        /// Connection details can be found in the config file.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TaskFormLoad(object sender, EventArgs e)
        {
            string logDirectory = Properties.Settings.Default.LogDirectory;

            // if log directory doesn't exist, make it
            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            SetLogPath(logDirectory);

            if (Properties.Settings.Default.Log)
            {
                sapient_logger = SapientLogger.CreateLogger(Properties.Settings.Default.LogDirectory, Properties.Settings.Default.LogPrefix, Properties.Settings.Default.IncrementIntervalSeconds);
            }

            var client = new SapientClient(Properties.Settings.Default.HldmmDataAgentAddress, Properties.Settings.Default.HldmmDataAgentPort);

            connection = client;
            messenger = client;
            messenger.SetNoDelay(true);
            messenger.SetSendNullTermination(Properties.Settings.Default.sendNullTermination);

            // if only sending data, currently the SapientComms Librray won't reconnect
            // This isn't a problem as we need bi-directional comms
            const bool SEND_ONLY_CONNECTION = false;
            connection.Start(1024 * 1024, SEND_ONLY_CONNECTION);
            connection.SetDataReceivedCallback(DataCallback);
            Log.InfoFormat("Port: {0}", Properties.Settings.Default.HldmmDataAgentPort);
            Text = Text + ": " + Properties.Settings.Default.HldmmDataAgentPort;
        }

        /// <summary>
        /// Callback triggered on receipt of message from middleware
        /// </summary>
        /// <param name="msg_buffer">message contents</param>
        /// <param name="size">message size</param>
        /// <param name="client">client comms connection</param> 
        void DataCallback(ref byte[] msg_buffer, Int32 size, IConnection client)
        {
            var message = System.Text.Encoding.UTF8.GetString(msg_buffer, 0, size);
            message = message.TrimStart('\r', '\n', '\0', ' ');
            if (message.Contains("</SensorTaskACK>"))
            {
                try
                {
                    TaskACKParser.ParseTaskACK(message);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Parse Task ACK Failed: {0} {1}", message, ex);
                    LogError(msg_buffer, size);
                }
            }
            else if (message.Contains("</Error>"))
            {
                try
                {
                    Log.Debug("Error Message Received");
                    var e = (Error)ConfigXMLParser.Deserialize(typeof(Error), message);
                    UpdateOutputWindow("Error: " + e.errorMessage);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Error Message Failed ", ex);
                }
            }
            else if (message.Contains("</SensorTask>"))
            {
                try
                {
                    Log.Info("Task Message Received");
                    var task = (SensorTask)ConfigXMLParser.Deserialize(typeof(SensorTask), message);
                    GenerateTaskAck(task.sensorID, task.taskID);
                    UpdateOutputWindow("SensorTask Received ID: " + task.taskID.ToString());
                    Log.InfoFormat("Task ID: {0}", task.taskID);
                    Log.Info(message);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Task Message Failed ", ex);
                }
            }
            else if (message.Contains("</AlertResponse>"))
            {
                try
                {
                    Log.Info("AlertResponse Message Received");
                    var response = (AlertResponse)ConfigXMLParser.Deserialize(typeof(AlertResponse), message);
                    UpdateOutputWindow("AlertResponse Received alertID: " + response.alertID.ToString());
                    Log.InfoFormat("AlertResponse alertID: {0}", response.alertID);
                    Log.Info(message);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse AlertResponse Message Failed ", ex);
                }
            }
            else if (message.Contains("</Alert>"))
            {
                try
                {
                    Log.Info("Alert Message Received");
                    var alert = (Alert)ConfigXMLParser.Deserialize(typeof(Alert), message);
                    UpdateOutputWindow("Alert Received alertID: " + alert.alertID.ToString() + " sensor:" + alert.sourceID.ToString());
                    Log.InfoFormat("Alert alertID: {0} from sensor: {1}", alert.alertID, alert.sourceID);
                    Log.Info(message);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Alert Message Failed ", ex);
                }
            }
            else if (message.Contains("</Approval>"))
            {
                try
                {
                    Log.Info("Approval Message Received");
                    var approval = (Approval)ConfigXMLParser.Deserialize(typeof(Approval), message);
                    UpdateOutputWindow("Approval Received ObjectiveID: " + approval.objectiveID.ToString() + " sensor:" + approval.sensorID.ToString() + " Status:" + approval.status);
                    Log.InfoFormat("Approval objectiveID: {0} from sensor: {1} with status: {2}", approval.objectiveID, approval.sensorID, approval.status);
                    Log.Info(message);
                }
                catch (Exception ex)
                {
                    Log.Error("Parse Alert Message Failed ", ex);
                }
            }
            else if (message.Length > 0)
            {
                Log.InfoFormat("Unknown Message of length {0} Received:{1}", message.Length, message);
            }
        }

        /// <summary>
        /// On click event from UI, event to send task to server 
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SendTaskClick(object sender, EventArgs e)
        {
            int sensorId = int.Parse(Sensor_input.Text);
            SendCommandOpenDialog(false, sensorId);
        }

        /// <summary>
        /// Open PTZ dislog and select command to send
        /// </summary>
        /// <param name="ptzCommand"></param>
        private void SendCommandOpenDialog(bool ptzCommand, int sensorId)
        {
            PTZForm ptzform = new PTZForm(ptzCommand, sensorId, this);
            DialogResult result = ptzform.ShowDialog();
            if (result == DialogResult.OK)
            {
                SendCommand(PTZForm.SensorID, ptzform.command.Text);
            }
        }

        /// <summary>
        /// send command
        /// </summary>
        public void SendCommand(int sensorId, string command)
        {
            int numSensors = 1;
            sensorId = PTZForm.SensorID;

            if (command == "lookAt")
            {
                this.lookAtTaskGenerator.GeneratePTZTask(PTZForm.Az, PTZForm.Elevation, PTZForm.PTZ, PTZForm.Zoom, messenger, sensorId, numSensors);
            }
            else if (command == "lookAt-XY")
            {
                double x = Properties.Settings.Default.startLongitude; /// TODO Add PTZForm.XValue1;
                double y = Properties.Settings.Default.startLatitude; //// TODO Add PTZForm.YValue1;
                double z = 0; //// TODO Add PTZForm.ZValue1;
                bool useZ = false; // TODO Add PTZForm.UseZ;
                this.xyzLookAtTaskGenerator.GenerateXYZLookAtTask(x, y, z, useZ, messenger, sensorId, numSensors);
            }
            else if (command == "region")
            {
                double x = Properties.Settings.Default.startLongitude; /// TODO Add PTZForm.XValue1;
                double y = Properties.Settings.Default.startLatitude; //// TODO Add PTZForm.YValue1;
                double z = 0; //// TODO Add PTZForm.ZValue1;
                bool useZ = false; // TODO Add PTZForm.UseZ;
                this.regionTaskGenerator.Generate(x, y, z, useZ, messenger, sensorId, numSensors);
            }
            else
            {
                SendTaskLoop(command, sensorId, numSensors);
            }
        }

        /// <summary>
        /// On click event from UI, event to show the PTZ form and send PTZ task to server 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendPTZTaskClick(object sender, EventArgs e)
        {
            int sensorId = int.Parse(Sensor_input.Text);
            SendCommandOpenDialog(true, sensorId);
        }


        /// <summary>
        /// method to Update Output Window that can be called from outside the UI thread
        /// </summary>
        /// <param name="message">message to update with</param>
        public void UpdateOutputWindow(string message)
        {
            if (Output_box.InvokeRequired)
            {
                Output_box.Invoke(new UpdaterCallback(StringUpdater), message);
            }
            else
            {
                StringUpdater(message);
            }
        }

        /// <summary>
        /// function and delegate for updating the GUI from outside the UI thread
        /// </summary>
        /// <param name="text">message to print on GUI</param>
        public void StringUpdater(string text)
        {
            Output_box.AppendText(text + "\r\n");
        }

        /// <summary>
        /// delegate for Updater Callback
        /// </summary>
        /// <param name="t">text to update output window with</param>
        public delegate void UpdaterCallback(string t);

        /// <summary>
        /// On click event from UI Send File button
        /// selects a file and sends its content as a message 
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ReadFileClick(object sender, EventArgs e)
        {
            var result = open_file_dialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                var xml_file_location = open_file_dialog.FileName;
                ReadAll(xml_file_location);
                open_file_dialog.InitialDirectory = Path.GetDirectoryName(open_file_dialog.FileName);
            }
        }

        /// <summary>
        /// Method to read xml from file.
        /// Checking could be done here to make sure XML is correct 
        /// However for testing this is not being done
        /// </summary>
        /// <param name="input_filename">Path to XML file to be loaded.</param>
        private void ReadAll(string input_filename)
        {
            using (var sr = new StreamReader(input_filename))
            {
                var whole = sr.ReadToEnd();
                var genreal = System.Text.Encoding.UTF8.GetBytes(whole);
                if (messenger.SendMessage(genreal, genreal.Length))
                {
                    UpdateOutputWindow("Sent: " + Path.GetFileName(input_filename));
                }
                else
                {
                    UpdateOutputWindow("Send Failed: " + Path.GetFileName(input_filename));
                }
            }
        }

        /// <summary>
        /// On click event from UI Clear button
        /// clears the output text box
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ClearClick(object sender, EventArgs e)
        {
            Output_box.Text = string.Empty;
        }

        /// <summary>
        /// Send an example detection message generated by hldmm
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SendDetectionClick(object sender, EventArgs e)
        {
            if (background_detections == null || background_detections.IsAlive == false)
            {
                this.detectionGenerator.LoopTime = 100;
                background_detections = new Thread(o => this.detectionGenerator.GenerateHLDetections(o));
                background_detections.Start(messenger);
            }
        }

        /// <summary>
        /// Form Closed Event from UI
        /// shuts down the connection
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TaskFormFormClosed(object sender, FormClosedEventArgs e)
        {
            connection.Shutdown();
        }

        /// <summary>
        /// Send an alert response with the specified alert id to the selected sensor
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void AlertResponseClick(object sender, EventArgs e)
        {
            var alert_response = new AlertResponse
            {
                sourceID = int.Parse(Sensor_input.Text),
                alertID = int.Parse(alertID.Text),
                reason = "Reason",
                status = "Acknowledge",
                timestamp = DateTime.UtcNow,
            };
            var xmlstring = ConfigXMLParser.Serialize(alert_response);

            UpdateOutputWindow("Send Alert Response:" + xmlstring);
            var record_bytes = System.Text.Encoding.UTF8.GetBytes(xmlstring);
            if (messenger.SendMessage(record_bytes, record_bytes.Length))
            {
                if (sapient_logger != null && Properties.Settings.Default.Log) sapient_logger.Log(xmlstring);
                Log.Info("Send Alert Response Succeeded");
            }
            else
            {
                Log.Error("Send Alert Response Failed");
            }
        }

        /// <summary>
        /// Send taskACK message
        /// </summary>
        /// <param name="sensorId">sensor ID form task message</param>
        /// <param name="taskId">task ID from task message</param>
        public void GenerateTaskAck(int sensorId, int taskId)
        {
            var sensor_task_ack = new SensorTaskACK
            {
                sensorID = sensorId,
                taskID = taskId,
                timestamp = DateTime.UtcNow,
                status = "Accepted",
            };
            var xmlstring = ConfigXMLParser.Serialize(sensor_task_ack);
            File.WriteAllLines(@".\SensorTaskACK1.xml", new[] { xmlstring });
            var ack = System.Text.Encoding.UTF8.GetBytes(xmlstring);
            UpdateOutputWindow("SensorTaskACK Sent");
            Log.Info(messenger.SendMessage(ack, ack.Length)
                                  ? "Send Task Ack Succeeded"
                                  : "Send Task Ack  Failed");
        }

        /// <summary>
        /// Send High Level Status message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HLHeartbeatButton_Click(object sender, EventArgs e)
        {
            HeartbeatGenerator heartbeatGenerator = new HeartbeatGenerator();
            heartbeatGenerator.GenerateHLStatus(messenger);
        }

        private void HLAlertButton_Click(object sender, EventArgs e)
        {
            this.alertGenerator.GenerateAlert(messenger);
        }

        /// <summary>
        /// Send list of files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileScriptButton_Click(object sender, EventArgs e)
        {
            ScriptReader.ScriptForm scriptForm = new ScriptReader.ScriptForm(this.messenger);
            this.connection.SetDataReceivedCallback(scriptForm.DataCallback);
            scriptForm.ShowDialog();
        }

        /// <summary>
        /// Send an example task message in a loop
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SendTaskLoop(string commandString, int sensorId, int numSensors)
        {
            if (background_tasks == null || background_tasks.IsAlive == false)
            {
                taskGenerator.LoopTime = 100;
                taskGenerator.CommandString = commandString;
                taskGenerator.BaseSensorID = sensorId;
                taskGenerator.NumSensors = numSensors;
                background_tasks = new Thread(o => taskGenerator.GenerateCommand(o));
                background_tasks.Start(messenger);
            }
        }

        private void LogError(byte[] bytes, int length)
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            while (i < length)
            {
                byte b = bytes[length];
                sb.AppendFormat("{0:X},", b);
                if (b == 0xa)
                {
                    Log.ErrorFormat("LogError: {0}", sb.ToString());
                    sb.Clear();
                }
                i++;
            }
            Log.ErrorFormat("LogError:end: {0}", sb.ToString());
        }

        private void loopTasksCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.loopTasksCheckBox.Checked == false)
            {
                try
                {
                    background_tasks.Abort();
                }
                catch { }
                taskGenerator.LoopMessages = false;
            }
            else
            {
                taskGenerator.LoopMessages = true;
            }
        }

        /// <summary>
        /// set log path for log4net
        /// </summary>
        /// <param name="logDirectory"></param>
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
                    string logFileLocation = System.IO.Path.Combine(logDirectory, "hldmmlog.txt");

                    fa.File = logFileLocation;
                    fa.ActivateOptions();
                    break;
                }
            }
        }

        /// <summary>
        /// Toggle use of loop send of detection messages - more intuitive than previous implementation
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        private void loopDetectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.loopDetectionCheckBox.Checked == false)
            {
                try
                {
                    background_tasks.Abort();
                }
                catch { }
                detectionGenerator.LoopMessages = false;
            }
            else
            {
                detectionGenerator.LoopMessages = true;
            }
        }

        /// <summary>
        /// Send objective messages to middleware
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void sendObjectiveButton_Click(object sender, EventArgs e)
        {
            int sensorId = int.Parse(Sensor_input.Text);
            this.objectiveGenerator.GenerateObjectives(messenger, sensorId);
        }

        private void filenameTextBox_TextChanged(object sender, EventArgs e)
        {
            SetAssociatedFilename(filenameTextBox.Text);
        }

        /// <summary>
        /// Set associated filename for detections and alerts
        /// </summary>
        /// <param name="filename">filename string</param>
        private void SetAssociatedFilename(string filename)
        {
            if (this.detectionGenerator != null)
            {
                this.detectionGenerator.ImageURL = filename;
            }

            AlertGenerator.ImageURL = filename;
        }

        private void sendToHLDMMcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool sendToHldmm = this.sendToHLDMMcheckBox.Checked;
            TaskGenerator.SendToHldmm = sendToHldmm; // set for all classes that inherit from TaskGenerator
        }
    }
}