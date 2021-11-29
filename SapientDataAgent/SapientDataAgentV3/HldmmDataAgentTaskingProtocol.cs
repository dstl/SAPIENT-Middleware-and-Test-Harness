// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: HldmmDataAgentTaskingProtocol.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using log4net;
    using SapientDatabase;
    using SapientServices;
    using SapientServices.Data;

    /// <summary>
    /// class to handle the Sapient HLDMM Data Agent Tasking Protocol.
    /// </summary>
    public class HldmmDataAgentTaskingProtocol : SapientProtocol
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly new ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// time histogram of database latency.
        /// </summary>
        private Histogram databaseLatencyHistogram;

        /// <summary>
        /// time histogram of communication latency.
        /// </summary>
        private Histogram communicationLatencyHistogram;

        /// <summary>
        /// Holds task permission information.
        /// </summary>
        private TaskPermissions taskPermissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HldmmDataAgentTaskingProtocol" /> class.
        /// </summary>
        public HldmmDataAgentTaskingProtocol(TaskPermissions taskPermissions)
        {
            this.taskPermissions = taskPermissions;
            databaseLatencyHistogram = new Histogram();
            communicationLatencyHistogram = new Histogram();

            // Specify the messages supported by this connection.
            Supported.Add(SapientMessageType.Detection);
            Supported.Add(SapientMessageType.Status);
            Supported.Add(SapientMessageType.Alert);
            Supported.Add(SapientMessageType.Task);
            Supported.Add(SapientMessageType.TaskACK);
            Supported.Add(SapientMessageType.AlertResponse);
            Supported.Add(SapientMessageType.Objective);
        }

        /// <summary>
        /// Send response message triggered from parsed message.
        /// </summary>
        /// <param name="output">parsing result.</param>
        /// <param name="connection">connection message received on.</param>
        public override void SendResponse(SapientMessageType output, uint connection)
        {
            switch (output)
            {
                case SapientMessageType.Task:
                    var task = Encoding.UTF8.GetBytes(TaskMessage);
                    uint client_connection;
                    lock (Program.ClientMessageParsers)
                    {
                        var sensorId = ((SensorTask)ConfigXMLParser.Deserialize(typeof(SensorTask), TaskMessage)).sensorID;
                        client_connection = (from asm_message_handler in Program.ClientMessageParsers
                                             where asm_message_handler.Value.SapientProtocol.SensorIds.Contains(sensorId)
                                             select asm_message_handler.Key).Single();
                    }

                    ForwardMessage(task, Program.ClientComms, client_connection, "Sensor Task Forwarded", "Sensor Task Forward failed");

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.AlertResponse:
                    var alert = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        var sensorId = ((AlertResponse)ConfigXMLParser.Deserialize(typeof(AlertResponse), TaskMessage)).sourceID;
                        client_connection = (from asm_message_handler in Program.ClientMessageParsers
                                             where asm_message_handler.Value.SapientProtocol.SensorIds.Contains(sensorId)
                                             select asm_message_handler.Key).Single();
                    }

                    ForwardMessage(alert, Program.ClientComms, client_connection, "Alert Response Forwarded", "Alert Response Forward failed");

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.AlertResponse);
                    break;

                case SapientMessageType.ResponseIdError:
                    var error = new Error
                    {
                        packet = TaskMessage,
                        errorMessage = "No ASM with this ID",
                        timestamp = DateTime.UtcNow,
                    };

                    var xml_bytes1 = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(error));
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(xml_bytes1, Program.TaskingCommsConnection, "Error");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Log.ErrorFormat("Invalid response message received from HLDMM:Error on response ID: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.IdError:
                    var task_message = (SensorTask)ConfigXMLParser.Deserialize(typeof(SensorTask), TaskMessage);
                    var tack = new SensorTaskACK
                    {
                        sensorID = task_message.sensorID,
                        taskID = task_message.taskID,
                        status = "Rejected",
                        reason = "No ASM with this ID",
                        timestamp = DateTime.UtcNow,
                    };

                    var xml_bytes = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(tack));
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(xml_bytes, Program.TaskingCommsConnection, "Sensor Task Ack");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Log.ErrorFormat("Invalid message received from HLDMM:Error on ID: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.InvalidTasking:
                    var e_task = new Error
                    {
                        timestamp = DateTime.UtcNow,
                        packet = TaskMessage,
                        errorMessage = ErrorString,
                    };
                    var error_task = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(e_task));

                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(error_task, Program.TaskingCommsConnection, "Error");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Log.ErrorFormat("Invalid message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InvalidTasking);
                    break;

                case SapientMessageType.Unknown:
                    e_task = new Error
                    {
                        timestamp = DateTime.UtcNow,
                        packet = TaskMessage,
                        errorMessage = "Unrecognised message received",
                    };
                    error_task = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(e_task));

                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(error_task, Program.TaskingCommsConnection, "Error");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Log.ErrorFormat("Unrecognised message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.Detection:
                case SapientMessageType.Status:
                case SapientMessageType.Alert:
                case SapientMessageType.Objective:
                case SapientMessageType.TaskACK:
                    Program.MessageMonitor.IncrementMessageCount(output);
                    break;

                case SapientMessageType.SensorTaskDropped:
                    // Do we want to record this as a 'task' or add a new category for dropped tasks?
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.InternalError:
                    Log.ErrorFormat("HDA Task Error: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InternalError);
                    break;

                case SapientMessageType.Unsupported:
                    Log.ErrorFormat("Unsupported Message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                default:
                    Log.ErrorFormat("Unknown Middleware Response: {0}", output);
                    break;
            }
        }

        /// <summary>
        /// Parses and processes a detection report message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessDetectionReport(string message, Database database)
        {
            Log.Info("HL Detection Message Received:");
            try
            {
                DetectionReport detectionReport;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseDetection(message, out detectionReport, out errorString);

                if (retval == SapientMessageType.Detection)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    database.DbDetection(detectionReport, true);

                    // measure communications and database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                    double latency = (DateTime.UtcNow - detectionReport.timestamp).TotalMilliseconds;
                    communicationLatencyHistogram.Add(latency);

                    Log.Info("DetectionReport Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                    Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());

                    // Don't update latency based on detection reports as these are dependent on original data so there is additional latency to just comms.
                    Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                }

                // parser method returns InvalidClient but we need InvalidTasking to show the source of the error.
                if (retval == SapientMessageType.InvalidClient)
                {
                    retval = SapientMessageType.InvalidTasking;
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL DetectionReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a status report message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessStatusReport(string message, Database database)
        {
            Log.Info("HL Status Report Message Received");
            try
            {
                StatusReport statusReport;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseStatusReport(message, out statusReport, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Status)
                {
                    string sensorStatus = statusReport.system;
                    Program.MessageMonitor.SetStatusText(1, sensorStatus);

                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // write data to database.
                    database.DbHLStatus(statusReport);

                    // measure communications and database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                    double latency = (DateTime.UtcNow - statusReport.timestamp).TotalMilliseconds;
                    communicationLatencyHistogram.Add(latency);

                    Log.Info("StatusReport Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                    Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                    Program.MessageMonitor.SetLatency(0, latency);
                    Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                }

                // parser method returns InvalidClient but we need InvalidTasking to show the source of the error.
                if (retval == SapientMessageType.InvalidClient)
                {
                    retval = SapientMessageType.InvalidTasking;
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL Status Report:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes an alert message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessAlert(string message, Database database)
        {
            Log.Info("HL Alert Message Received:");
            try
            {
                Alert alert;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseAlert(message, out alert, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Alert)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // write data to database.
                    database.DbAlert(alert, true);

                    // measure communications and database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                    double latency = (DateTime.UtcNow - alert.timestamp).TotalMilliseconds;
                    communicationLatencyHistogram.Add(latency);

                    Log.Info("Alert Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                    Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                    Program.MessageMonitor.SetLatency(0, latency);
                    Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                }

                // parser method returns InvalidClient but we need InvalidTasking to show the source of the error.
                if (retval == SapientMessageType.InvalidClient)
                {
                    retval = SapientMessageType.InvalidTasking;
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL Alert:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a sensor task message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessSensorTask(string message, Database database)
        {
            Log.Info("Sensor Task Message Received");
            try
            {
                SensorTask task;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseSensorTask(message, out task, out errorString);
                ErrorString = errorString;
                if (retval == SapientMessageType.Task)
                {
                    var sensorId = task.sensorID;
                    if (taskPermissions.HldmmHasControl(sensorId))
                    {
                        bool foundClient;
                        lock (Program.ClientMessageParsers)
                        {
                            foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                        }

                        if (!foundClient)
                        {
                            ErrorString = string.Format("No ASM with this ID: {0}", sensorId);
                            retval = SapientMessageType.IdError;
                        }
                    }
                    else
                    {
                        // GUI has control over this sensor, so the HLDMM will drop sensor tasks from it.
                        Log.Info($"GUI currently has control of sensor with ID {sensorId}. Rejecting this task.");

                        retval = SapientMessageType.SensorTaskDropped;
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL SensorTask:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a sensor task acknowledgement message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessSensorTaskAck(string message, Database database)
        {
            Log.Info("Sensor Task Ack Message Received from HLDMM");
            try
            {
                SensorTaskACK sensorTaskAck;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseSensorTaskAck(message, out sensorTaskAck, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.TaskACK)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // write sensor_task_ack to database.
                    database.DbAcknowledgeTask(sensorTaskAck, SapientDatabase.DatabaseTables.TaskConstants.HLTaskAck.Table);

                    // measure database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    stopWatch.Stop();
                    Log.Info("HL TaskAck Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms");
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL SensorTaskACK:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes an alert response message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessAlertResponse(string message, Database database)
        {
            Log.Info("Alert Response Message Received");
            try
            {
                AlertResponse alertResponse;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseAlertResponse(message, out alertResponse, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.AlertResponse)
                {
                    var sensorId = alertResponse.sourceID;
                    bool foundClient;
                    lock (Program.ClientMessageParsers)
                    {
                        foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                    }

                    if (!foundClient)
                    {
                        Log.Warn("AlertResponse:Sensor Id not on record");
                        retval = SapientMessageType.ResponseIdError;
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL AlertResponse:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Objective Message and return success or otherwise.
        /// </summary>
        /// <param name="message">message string.</param>
        /// <param name="database">database to write to.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessObjective(string message, Database database)
        {
            Log.Info("Objective Message Received");
            Log.Info(message);

            try
            {
                Objective objective;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseObjective(message, out objective, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Objective)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // write data to database.
                    database.DbObjective(objective);

                    // Add objective to objective information store.
                    ObjectiveInformation.AddObjective(objective);

                    // measure communications and database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                    double latency = (DateTime.UtcNow - objective.timestamp).TotalMilliseconds;
                    communicationLatencyHistogram.Add(latency);

                    Log.Info("Objective Message Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                    Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                    Program.MessageMonitor.SetLatency(0, latency);
                    Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:HL Objective:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }
    }
}
