// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: HldmmDataAgentClientProtocol.cs$
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
    /// class to handle the Sapient HLDMM Data Agent Client Protocol.
    /// </summary>
    public class HldmmDataAgentClientProtocol : SapientProtocol
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly new ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Fixed ASM id specified in the config file.
        /// </summary>
        private int fixedAsmId;

        /// <summary>
        /// client sensor Identifier.
        /// </summary>
        private int sensorID;

        /// <summary>
        /// Initializes a new instance of the <see cref="HldmmDataAgentClientProtocol" /> class.
        /// </summary>
        /// <param name="fixedASMId">fixed ASM ID from config file.</param>
        public HldmmDataAgentClientProtocol(int fixedASMId)
        {
            this.fixedAsmId = fixedASMId;

            // Specify the messages supported by this connection
            Supported.Add(SapientMessageType.Registration);
            Supported.Add(SapientMessageType.Alert);
            Supported.Add(SapientMessageType.TaskACK);
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
                case SapientMessageType.Registration:
                    var srack = new SensorRegistrationACK { sensorID = this.sensorID };
                    lock (Program.ClientMessageParsers)
                    {
                        if (SensorIds.Contains(sensorID) == false)
                        {
                            SensorIds.Add(sensorID);
                        }
                    }

                    var message = ConfigXMLParser.Serialize(srack);

                    var send = Encoding.UTF8.GetBytes(message);
                    ForwardMessage(send, Program.ClientComms, connection, "Sensor Registration Ack Forwarded", "Sensor Registration Ack Forward failed");

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Registration);
                    break;

                case SapientMessageType.TaskACK:
                    var xml_bytes = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(xml_bytes, Program.TaskingCommsConnection, "Sensor Task Ack");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.TaskACK);
                    break;

                case SapientMessageType.Alert:
                    var alert_xml_bytes = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(alert_xml_bytes, Program.TaskingCommsConnection, "Alert");
                    }
                    else
                    {
                        Log.Warn("HLDMM not connected");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Alert);
                    break;

                case SapientMessageType.IdError:
                    var error_message = new Error
                    {
                        timestamp = DateTime.UtcNow,
                        packet = TaskMessage,
                        errorMessage = ErrorString,
                    };
                    var error = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(error_message));
                    ForwardMessage(error, Program.ClientComms, connection, "Invalid Message - Error returned: " + ErrorString, "Invalid message - Error return failed: " + ErrorString);
                    Log.ErrorFormat("Invalid message received from SDA:Error on ID: {0}", ErrorString);

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.InvalidClient:
                    Log.ErrorFormat("Invalid message received from SDA:InvalidClient: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InvalidClient);
                    break;

                case SapientMessageType.Unknown:
                    Log.ErrorFormat("Unrecognised message received from SDA: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.InternalError:
                    Log.ErrorFormat("HDA Client Error: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InternalError);
                    break;

                case SapientMessageType.Unsupported:
                    Log.ErrorFormat("Unsupported Message received from SDA: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                default:
                    Log.ErrorFormat("Unknown Middleware Response: {0}", output);
                    break;
            }
        }

        /// <summary>
        /// Parses and processes a sensor registration message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessRegistration(string message, Database database)
        {
            Log.Info("Sensor Registration Message Received");
            try
            {
                SensorRegistration registration;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseRegistration(message, out registration, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Registration)
                {
                    if (registration.sensorIDSpecified == false)
                    {
                        lock (Program.ClientMessageParsers)
                        {
                            var all_ids = Program.ClientMessageParsers.Values.SelectMany(a => a.SapientProtocol.SensorIds);
                            if (fixedAsmId > 0)
                            {
                                sensorID = (int)ConnectionId;
                            }
                            else
                            {
                                sensorID = all_ids.Any() ? all_ids.Max() + 1 : 1;
                            }

                            registration.sensorID = sensorID;
                        }
                    }
                    else
                    {
                        var sensor_id = registration.sensorID;
                        lock (Program.ClientMessageParsers)
                        {
                            var all_ids = Program.ClientMessageParsers.Values.SelectMany(a => a.SapientProtocol.SensorIds);
                            sensorID = sensor_id;
                            if (SensorIds.Contains(sensor_id))
                            {
                            }
                            else if (all_ids.Contains(sensor_id))
                            {
                                // id in use by another client
                                ErrorString = "Another ASM is using this ID: " + sensor_id;
                                retval = SapientMessageType.IdError;
                            }
                        }
                    }

                    database.DbRegistration(registration, message);
                    if (AdditionalDatabase != null)
                    {
                        AdditionalDatabase.DbRegistration(registration, message);
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:SensorRegistration:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes an alert message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">database to write to.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessAlert(string message, Database database)
        {
            Log.Info("Alert Message Received:");
            try
            {
                Alert alert;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseAlert(message, out alert, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Alert)
                {
                    var sourceId = alert.sourceID;
                    bool found;
                    lock (Program.ClientMessageParsers)
                    {
                        found = SensorIds.Contains(sourceId);
                    }

                    if (!found)
                    {
                        ErrorString = string.Format("No ASM with this ID: {0}", sourceId);
                        retval = SapientMessageType.IdError;
                    }

                    // if OK, do nothing other than forward on message in Send Response.
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:Alert:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a sensor task acknowledgement message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">database to write to.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessSensorTaskAck(string message, Database database)
        {
            Log.Info("Sensor Task Ack Message Received");
            string errorString;
            SensorTaskACK sensor_task_ack;
            SapientMessageType retval = SapientMessageValidator.ParseSensorTaskAck(message, out sensor_task_ack, out errorString);
            ErrorString = errorString;
            return retval;
        }
    }
}
