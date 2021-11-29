// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientProtocol.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using log4net;
    using SapientDatabase;
    using SapientServices;
    using SapientServices.Communication;

    /// <summary>
    /// abstract class to support the Sapient Message Protocol
    /// </summary>
    public abstract class SapientProtocol
    {
        protected readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string TaskMessage;
        protected string ErrorString;

        /// <summary>
        /// List of supported message typw
        /// </summary>
        protected HashSet<SapientMessageType> Supported = new HashSet<SapientMessageType>();

        public List<int> SensorIds = new List<int>();

        public uint ConnectionId;

        /// <summary>
        /// Additional database for writing to
        /// </summary>
        public static Database AdditionalDatabase { get; set; }

        /// <summary>
        /// Parse messages and write contents to database if required
        /// </summary>
        /// <param name="message">received message</param>
        /// <param name="database">database connection</param>
        /// <param name="messageType">message type</param>
        /// <returns>parse result</returns>
        public virtual SapientMessageType ParseReceivedMessage(string message, Database database, int messageType)
        {
            Log.Debug("Called with message: " + message);

            message = message.TrimStart('\r', '\n', '\0', ' ');
            TaskMessage = message;

            SapientMessageType msgType = (SapientMessageType)messageType;
            SapientMessageType parsedMsgType = SapientMessageType.Unsupported;

            if (Supported.Contains(msgType))
            {
                switch (msgType)
                {
                    case SapientMessageType.Detection:
                        parsedMsgType = ProcessDetectionReport(message, database);
                        break;

                    case SapientMessageType.Status:
                        parsedMsgType = ProcessStatusReport(message, database);
                        break;

                    case SapientMessageType.Alert:
                        parsedMsgType = ProcessAlert(message, database);
                        break;

                    case SapientMessageType.TaskACK:
                        parsedMsgType = ProcessSensorTaskAck(message, database);
                        break;

                    case SapientMessageType.Registration:
                        parsedMsgType = ProcessRegistration(message, database);
                        break;

                    case SapientMessageType.RegistrationACK:
                        parsedMsgType = ProcessRegistrationAck(message, database);
                        break;

                    case SapientMessageType.Task:
                        parsedMsgType = ProcessSensorTask(message, database);
                        break;

                    case SapientMessageType.AlertResponse:
                        parsedMsgType = ProcessAlertResponse(message, database);
                        break;

                    case SapientMessageType.Error:
                        parsedMsgType = ParseErrorMessage(message);
                        break;

                    case SapientMessageType.Objective:
                        parsedMsgType = ProcessObjective(message, database);
                        break;

                    case SapientMessageType.RoutePlan:
                        parsedMsgType = ProcessRoutePlan(message, database);
                        break;

                    case SapientMessageType.Approval:
                        parsedMsgType = ProcessApproval(message, database);
                        break;

                    default:
                        Log.Info("Unrecognised Message Received:\r\n" + message);
                        parsedMsgType = SapientMessageType.Unknown;
                        break;
                }
            }
            else
            {
                Log.Info("Unsupported Message Received");
            }

            return parsedMsgType;
        }

        /// <summary>
        /// Send response message triggered from parsed message
        /// </summary>
        /// <param name="output">parsing result</param>
        /// <param name="connection">connection message received on</param>
        public abstract void SendResponse(SapientMessageType output, uint connection);

        /// <summary>
        /// Wrapper around generic deserialize
        /// </summary>
        /// <param name="ser"> type of object to convert to</param>
        /// <param name="xml_string">xml string of the target class type</param>
        /// <param name="error">deserialization error message</param>
        /// <returns>deserialized object</returns>
        public static object Deserialize(XmlSerializer ser, string xml_string, ref string error)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(xml_string);
                var mem = new MemoryStream(bytes);
                return ser.Deserialize(mem);
            }
            catch (Exception e)
            {
                error = "Error Deserialize object: " + e.Message;
                return null;
            }
        }

        /// <summary>
        /// Forward message logging if succeed or fail
        /// </summary>
        /// <param name="send">message to send</param>
        /// <param name="connection">communications connection</param>
        /// <param name="message">message to log</param>
        protected void ForwardMessage(byte[] send, IConnection connection, string message)
        {
            if (connection.SendMessage(send, send.Length))
            {
                Log.Info(message + " Forwarded");
            }
            else
            {
                Log.Warn(message + " Forward failed");
            }
        }

        /// <summary>
        /// Forward message logging if succeed or fail
        /// </summary>
        /// <param name="send">message to send</param>
        /// <param name="server">communications server</param>
        /// <param name="connection_id">communications client connection</param>
        /// <param name="succeed">succeed message</param>
        /// <param name="fail">fail message</param>
        protected void ForwardMessage(byte[] send, SapientServer server, uint connection_id, string succeed, string fail)
        {
            if (server.SendMessage(send, send.Length, connection_id))
            {
                Log.Info(succeed);
            }
            else
            {
                Log.Warn(fail);
            }
        }

        protected virtual SapientMessageType ProcessRegistration(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessDetectionReport(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessStatusReport(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessAlert(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessSensorTask(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessRegistrationAck(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessSensorTaskAck(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessAlertResponse(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessObjective(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessRoutePlan(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ProcessApproval(string message, Database database)
        {
            return SapientMessageType.Unsupported;
        }

        protected virtual SapientMessageType ParseErrorMessage(string message)
        {
            Log.Warn("SAPIENT Error Message Received");
            return SapientMessageType.Error;
        }
    }
}
