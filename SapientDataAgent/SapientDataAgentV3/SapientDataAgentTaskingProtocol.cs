// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientDataAgentTaskingProtocol.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using System.Linq;
    using System.Text;
    using log4net;
    using SapientDatabase;
    using SapientServices;
    using SapientServices.Data;

    /// <summary>
    /// class to handle the Sapient Data Data Agent Tasking Protocol.
    /// </summary>
    public class SapientDataAgentTaskingProtocol : SapientProtocol
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly new ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientDataAgentTaskingProtocol" /> class.
        /// </summary>
        public SapientDataAgentTaskingProtocol()
        {
            // Specify the messages supported by this connection.
            Supported.Add(SapientMessageType.Task);
            Supported.Add(SapientMessageType.RegistrationACK);
            Supported.Add(SapientMessageType.AlertResponse);
            Supported.Add(SapientMessageType.Error);
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
                        client_connection = Program.ClientMessageParsers.Keys.Single();
                    }

                    ForwardMessage(task, Program.ClientComms, client_connection, "Sensor Task Forwarded", "Sensor Task Forward failed");
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Task);
                    break;

                case SapientMessageType.AlertResponse:
                    var response = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        client_connection = Program.ClientMessageParsers.Keys.Single();
                    }

                    ForwardMessage(response, Program.ClientComms, client_connection, "Alert Response Forwarded", "Alert Response Forward failed");
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.AlertResponse);
                    break;

                case SapientMessageType.Error:
                    var error = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        client_connection = Program.ClientMessageParsers.Keys.Single();
                    }

                    ForwardMessage(error, Program.ClientComms, client_connection, "Error Forwarded", "Error Forward failed");
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Error);
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
                        Log.Warn("HLDMM data agent not connected");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.RegistrationACK:
                    var sensor_reg_ack = Encoding.UTF8.GetBytes(TaskMessage);
                    lock (Program.ClientMessageParsers)
                    {
                        var sensor_id = ((SensorRegistrationACK)ConfigXMLParser.Deserialize(typeof(SensorRegistrationACK), TaskMessage)).sensorID;
                        client_connection = (from asm_message_handler in Program.ClientMessageParsers
                                             where asm_message_handler.Value.SapientProtocol.SensorIds.Contains(sensor_id)
                                             select asm_message_handler.Key).Single();
                    }

                    ForwardMessage(sensor_reg_ack, Program.ClientComms, client_connection, "Sensor Registration Ack Forwarded", "Sensor Registration Ack Forward failed");
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.RegistrationACK);
                    break;

                case SapientMessageType.InvalidTasking:
                    Log.ErrorFormat("Invalid message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InvalidTasking);
                    break;

                case SapientMessageType.Unknown:
                    Log.ErrorFormat("Unrecognised message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.InternalError:
                    Log.ErrorFormat("SDA Task Error: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InternalError);
                    break;

                case SapientMessageType.Unsupported:
                    Log.ErrorFormat("Unsupported Message received from HLDMM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                default:
                    Log.ErrorFormat("Unknown Middleware Response:{0}", output);
                    break;
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
            SapientMessageType retval = SapientMessageType.InvalidTasking;
            try
            {
                string deserialize_error = string.Empty;
                SensorTask task = (SensorTask)Deserialize(ConfigXMLParser.SensorTask, message, ref deserialize_error);
                int sensorId = task.sensorID;
                bool foundClient;

                lock (Program.ClientMessageParsers)
                {
                    foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                }

                if (foundClient && (task != null))
                {
                    try
                    {
                        // Task for ASM.
                        // When multiple ASMs are tasked simultaneously, writing the ACKs to the database at the same time causes concurrency errors.
                        // Therefore, move writing of task and ack from HLDMM Data Agent to Sensor Data Agent so that Acks from different sensors are written to the database on different connections.
                        database.DbTask(task, message, false);
                        retval = SapientMessageType.Task;
                    }
                    catch (Exception e)
                    {
                        ErrorString = "Internal:SensorTaskDB:" + e.Message;
                        retval = SapientMessageType.InternalError;
                    }
                }
                else
                {
                    ErrorString = string.Format("No ASM with this ID: {0}", sensorId);
                    Log.Warn("Sensor Id not on record");
                    retval = SapientMessageType.IdError;
                }
            }
            catch (Exception e)
            {
                ErrorString = "Internal:SensorTask:" + e.Message;
                retval = SapientMessageType.InternalError;
            }

            return retval;
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
            int sensorId;
            AlertResponse alert;
            SapientMessageType retval = SapientMessageType.InvalidTasking;
            try
            {
                alert = (AlertResponse)ConfigXMLParser.Deserialize(typeof(AlertResponse), message);
                sensorId = alert.sourceID;

                bool foundClient;
                lock (Program.ClientMessageParsers)
                {
                    foundClient = Program.ClientMessageParsers.Any(a => a.Value.SapientProtocol.SensorIds.Contains(sensorId));
                }

                if (foundClient)
                {
                    try
                    {
                        database.DbAcknowledgeAlert(alert);
                        retval = SapientMessageType.AlertResponse;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception parsing AlertResponse message", e);
                        ErrorString = "Internal:AlertResponseDB:" + e.Message;
                        retval = SapientMessageType.InternalError;
                    }
                }
                else
                {
                    Log.Warn("AlertResponse:Sensor Id not on record");
                    retval = SapientMessageType.IdError;
                }
            }
            catch (Exception e)
            {
                ErrorString = "Internal:AlertResponse:" + e.Message;
                retval = SapientMessageType.InternalError;
            }

            return retval;
        }

        /// <summary>
        /// Parses and processes a sensor registration acknowledgement message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessRegistrationAck(string message, Database database)
        {
            Log.Info("Sensor Registration Ack Message Received");
            SapientMessageType retval = SapientMessageType.InvalidTasking;
            var deserializeError = string.Empty;
            var data = (SensorRegistrationACK)Deserialize(ConfigXMLParser.SensorRegistrationACK, message, ref deserializeError);
            if (data != null)
            {
                try
                {
                    var sensorId = data.sensorID;
                    var client = Program.ClientMessageParsers.First();
                    if (client.Value.SapientProtocol.SensorIds.Contains(sensorId) == false)
                    {
                        client.Value.SapientProtocol.SensorIds.Add(sensorId);
                    }

                    string portText = string.Format("SDA:{0}: Port {1}", data.sensorID, Program.ClientPort);
                    Program.SetWindowText(portText);

                    retval = SapientMessageType.RegistrationACK;
                }
                catch (Exception e)
                {
                    Log.Error("Exception parsing SensorRegistrationACK message", e);
                    ErrorString = "Internal:SensorRegistrationACK:" + e.Message;
                    retval = SapientMessageType.InternalError;
                }
            }
            else
            {
                ErrorString = "Deserialize: " + deserializeError;
                retval = SapientMessageType.InvalidTasking;
            }

            return retval;
        }
    }
}
