// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: GUIProtocol.cs$
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
    /// Protocol for GUI connection.
    /// </summary>
    public class GUIProtocol : SapientProtocol
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly new ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Holds task permissions. This is both used and updated by GUIProtocol.
        /// </summary>
        private TaskPermissions taskPermissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GUIProtocol" /> class.
        /// </summary>
        public GUIProtocol(TaskPermissions taskPermissions)
        {
            this.taskPermissions = taskPermissions;

            // Specify the messages supported by this connection
            Supported.Add(SapientMessageType.Detection);
            Supported.Add(SapientMessageType.Task);
            Supported.Add(SapientMessageType.AlertResponse);
            Supported.Add(SapientMessageType.TaskACK);
            Supported.Add(SapientMessageType.Approval);
        }

        /// <summary>
        /// Send response message triggered from parsed message.
        /// </summary>
        /// <param name="output">The parsing result.</param>
        /// <param name="connection">The connection the message was received on.</param>
        public override void SendResponse(SapientMessageType output, uint connection)
        {
            switch (output)
            {
                case SapientMessageType.Task:
                    var task = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        SensorTask sensorTask = (SensorTask)ConfigXMLParser.Deserialize(typeof(SensorTask), TaskMessage);
                        var sensorId = sensorTask.sensorID;

                        if (sensorId == 0)
                        {
                            if ((sensorTask.taskName != null) && sensorTask.taskName.ToLowerInvariant().Contains("manual task"))
                            {
                                Log.Info("Forward manual task");

                                // if from GUI forward on to HLDMM.
                                if (Program.TaskingCommsConnection != null)
                                {
                                    ForwardMessage(task, Program.TaskingCommsConnection, "GUI-HLDMM Task");
                                }
                                else
                                {
                                    Log.Info("No HLDMM connected to task");
                                }
                            }
                            else
                            {
                                Log.Info("Task not forwarded - not manual task");
                            }
                        }
                        else
                        {
                            uint clientConnection = (from asm_message_handler in Program.ClientMessageParsers
                                                     where asm_message_handler.Value.SapientProtocol.SensorIds.Contains(sensorId)
                                                     select asm_message_handler.Key).Single();
                            ForwardMessage(task, Program.ClientComms, clientConnection, "Sensor Task Forward", "Sensor Task Forward failed");
                        }
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.SensorTaskDropped:
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.SensorTaskTakeControl:
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.SensorTaskReleaseControl:
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.AlertResponse:
                    var alert = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        var sensorId = ((AlertResponse)ConfigXMLParser.Deserialize(typeof(AlertResponse), TaskMessage)).sourceID;

                        if (sensorId == 0)
                        {
                            // if from GUI forward on to HLDMM
                            ForwardMessage(alert, Program.TaskingCommsConnection, "GUI-HLDMM Alert Response");
                        }
                        else
                        {
                            uint clientConnection = (from asm_message_handler in Program.ClientMessageParsers
                                                     where asm_message_handler.Value.SapientProtocol.SensorIds.Contains(sensorId)
                                                     select asm_message_handler.Key).Single();
                            ForwardMessage(alert, Program.ClientComms, clientConnection, "Alert Response Forwarded", "Alert Response Forward failed");
                        }
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.AlertResponse);
                    break;

                case SapientMessageType.TaskACK:
                    var sensorAck = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(sensorAck, Program.TaskingCommsConnection, "Sensor Task Ack");
                    }
                    else
                    {
                        Log.Info("No HLDMM connected to send task ack to");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.TaskACK);
                    break;

                case SapientMessageType.Detection:
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Detection);
                    break;

                case SapientMessageType.ResponseIdError:
                    Log.ErrorFormat("Invalid response message received from GUI:Error on response ID: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.IdError:
                    Log.ErrorFormat("Invalid message received from GUI:Error on ID: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.InvalidTasking:
                    Log.ErrorFormat("Invalid message received from GUI: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InvalidTasking);
                    break;

                case SapientMessageType.Unknown:
                    Log.ErrorFormat("Unrecognised message received from GUI: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.InternalError:
                    Log.ErrorFormat("GUI Client Error: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InternalError);
                    break;

                case SapientMessageType.Unsupported:
                    Log.ErrorFormat("Unsupported Message received from GUI: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.Approval:
                    var approval = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(approval, Program.TaskingCommsConnection, "Approval");
                    }
                    else
                    {
                        Log.Info("No HLDMM connected to forward approval to");
                    }
                    break;

                default:
                    Log.ErrorFormat("Unknown Middleware Response: {0}", output);
                    break;
            }
        }

        /// <summary>
        /// Parses and processes the detection report message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessDetectionReport(string message, Database database)
        {
            Log.Info("Detection Message Received from GUI");
            try
            {
                DetectionReport detectionReport;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseDetection(message, out detectionReport, out errorString);

                if (retval == SapientMessageType.Detection)
                {
                    // write detection to HL table in database.
                    database.DbDetection(detectionReport, true);
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
                ErrorString = "Internal:GUI:DetectionReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes the sensor task message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessSensorTask(string message, Database database)
        {
            Log.Info("Task Message Received from GUI");
            try
            {
                SensorTask task;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseSensorTask(message, out task, out errorString);
                ErrorString = errorString;
                if (retval == SapientMessageType.Task)
                {
                    var sensorId = task.sensorID;

                    if (IsTakeControl(task))
                    {
                        taskPermissions.TakeControl(sensorId);
                        retval = SapientMessageType.SensorTaskTakeControl;
                    }
                    else if (IsReleaseControl(task))
                    {
                        taskPermissions.ReleaseControl(sensorId);
                        retval = SapientMessageType.SensorTaskReleaseControl;
                    }
                    else
                    {
                        bool foundClient;
                        lock (Program.ClientMessageParsers)
                        {
                            foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                        }

                        // Handle GUI - HLDMM task.
                        if (sensorId == 0)
                        {
                            foundClient = true;
                        }

                        if (foundClient)
                        {
                            if (taskPermissions.GuiProtocolShouldForward(sensorId) || (sensorId == 0))
                            {
                                Log.Info($"GuiProtocol will forward task for sensor id {sensorId}");
                                retval = SapientMessageType.Task;

                                // Fudge for Zodiac Pre-alpha - create an objective for teh GUI to see for a manual task
                                GenerateManualTaskObjective(task, database);
                            }
                            else
                            {
                                Log.Info($"GuiProtocol rejected task for sensor id {sensorId}");

                                retval = SapientMessageType.SensorTaskDropped;
                            }
                        }
                        else
                        {
                            Log.Warn("GUI Task:Sensor Id not on record");
                            retval = SapientMessageType.IdError;
                        }

                        // write GUI task to HL_sensortask database table.
                        database.DbTask(task, message, true);
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:GUI:SensorTask:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes the alert response message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessAlertResponse(string message, Database database)
        {
            Log.Info("Alert Response Message Received from GUI");
            try
            {
                AlertResponse alertResponse;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseAlertResponse(message, out alertResponse, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.AlertResponse)
                {
                    bool foundClient;
                    var sensorId = alertResponse.sourceID;
                    lock (Program.ClientMessageParsers)
                    {
                        foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                    }

                    // Handle GUI - HLDMM responses
                    if (sensorId == 0)
                    {
                        foundClient = true;
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
                ErrorString = "Internal:GUI:AlertResponse:" + e.Message;
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
            Log.Info("Sensor Task Ack Message Received from GUI");
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

                    // write sensor_task_ack to database
                    database.DbAcknowledgeTask(sensorTaskAck, SapientDatabase.DatabaseTables.TaskConstants.GUITaskAck.Table);

                    // update objective information status from acknowledgement.
                    ObjectiveInformation.ApproveObjective(sensorTaskAck.taskID, sensorTaskAck.sensorID, sensorTaskAck.status, database);

                    // measure database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    stopWatch.Stop();
                    Log.Info("GUI TaskAck Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms");
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:GUI:SensorTaskACK:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a approval message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessApproval(string message, Database database)
        {
            Log.Info("Objective Approval Message Received from GUI");
            try
            {
                Approval approval;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseApproval(message, out approval, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Approval)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // update objective information status from acknowledgement.
                    ObjectiveInformation.ApproveObjective(approval.taskID, approval.sensorID, approval.status, database);
                    approval.information = database.GetObjectiveInformation(approval);
                    TaskMessage = ConfigXMLParser.Serialize(approval);
                    //approval.
                    // measure database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    stopWatch.Stop();
                    Log.Info("GUI TaskAck Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms");
                }
                message += "test";
                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:GUI:SensorTaskACK:" + e.Message;
                return SapientMessageType.InternalError;
            }

        }

        /// <summary>
        /// Checks if the sensor task command is 'release control'.
        /// </summary>
        /// <param name="task">The task to check.</param>
        /// <returns>True if the task command is release control.</returns>
        private static bool IsReleaseControl(SensorTask task)
        {
            bool retval = false;
            if ((task.command != null) && (task.command.request != null))
            {
                retval = task.command.request.Equals("Release Control", StringComparison.CurrentCultureIgnoreCase);
            }

            return retval;
        }

        /// <summary>
        /// Checks if the sensor task command is 'take control'.
        /// </summary>
        /// <param name="task">The task to check.</param>
        /// <returns>True if the task command is take control.</returns>
        private static bool IsTakeControl(SensorTask task)
        {
            bool retval = false;
            if ((task.command != null) && (task.command.request != null))
            {
                retval = task.command.request.Equals("Take Control", StringComparison.CurrentCultureIgnoreCase);
            }

            return retval;
        }

        private static void GenerateManualTaskObjective(SensorTask sensorTask, Database db)
        {
            if (sensorTask.sensorID == 0)
            {
                if ((sensorTask.taskName != null) && sensorTask.taskName.ToLowerInvariant().Contains("manual task"))
                {
                    int sensorId = 0;
                    if (sensorTask.command!=null)
                    {
                        sensorId = sensorTask.command.sensorID;
                    }

                    Log.InfoFormat("Creating a manual task objective for sensorID {0}", sensorId);

                    Objective objective = new Objective()
                    {
                        timestamp = sensorTask.timestamp,
                        sensorID = sensorId,
                        description = string.Format("manual task for sensor {0}", sensorId),
                    };

                    AddManualTaskObjectiveState(sensorTask, ref objective);

                    if (db != null)
                    {
                        db.DbObjective(objective);
                    }
                }
            }
            else
            {
                Log.Info("Not creating a manual task objective (non-zero sensorID)");
            }
        }

        private static void AddManualTaskObjectiveState(SensorTask sensorTask, ref Objective objective)
        {
            string taskStatus = "Active";
            string type = "Manual";
            double x = 0;
            double y = 0;
            double z = 0;

            if ((sensorTask.command != null) && (sensorTask.command.request != null) && (sensorTask.command.request.ToLowerInvariant() == "stop"))
            {
                taskStatus = "Stopped";

                if((sensorTask.command.lookAt!=null) && (sensorTask.command.lookAt.locationList!=null) && (sensorTask.command.lookAt.locationList.location[0]!=null))
                {
                    x = sensorTask.command.lookAt.locationList.location[0].X;
                    y = sensorTask.command.lookAt.locationList.location[0].Y;
                    z = sensorTask.command.lookAt.locationList.location[0].Z;
                }
            }

            objective.status = taskStatus;
            objective.objectiveType = type;
            objective.location = new location
            {
                X = x,
                Y = y,
                Z = z,
            };
        }
    }
}
