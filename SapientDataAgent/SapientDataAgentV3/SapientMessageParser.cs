// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientMessageParser.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace SapientMiddleware
{
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using SapientDatabase;
    using SapientServices;
    using SapientServices.Data;

    /// <summary>
    /// class to handle parsing of input messages from bytes via xml to class.
    /// </summary>
    public class SapientMessageParser
    {
        #region Data Definitions

        /// <summary>
        /// Underlying message protocol.
        /// </summary>
        public readonly SapientProtocol SapientProtocol;

        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// current message string buffer.
        /// </summary>
        private readonly StringBuilder currentMessage = new StringBuilder();

        /// <summary>
        /// message type as close tag.
        /// </summary>
        private string awaitedCloseTag;

        /// <summary>
        /// message type.
        /// </summary>
        private string currentTag;

        /// <summary>
        /// Whether to discard messages that do not have a null terminator.
        /// </summary>
        private bool discardUnterminatedMessages;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientMessageParser" /> class.
        /// </summary>
        /// <param name="sapient_protocol">protocol to apply.</param>
        /// <param name="discardUnterminatedMessages">whether to discard unterminated messages.</param>
        public SapientMessageParser(SapientProtocol sapient_protocol, bool discardUnterminatedMessages)
        {
            SapientProtocol = sapient_protocol;
            this.discardUnterminatedMessages = discardUnterminatedMessages;
        }

        /// <summary>
        /// Construct a complete XML message from received strings.
        /// </summary>
        /// <param name="msg_update">message received.</param>
        /// <param name="db">database connection.</param>
        /// <param name="connection_id">ID of connection.</param>
        public void BuildAndNotify(string msg_update, Database db, uint connection_id)
        {
            msg_update = msg_update.TrimStart('\r', '\n', '\0', ' ');
            var document_tag_name_pattern = new Regex(@"\?>\s*<(\w*)>");

            log.DebugFormat("Buffer length:{0}, New Message:{1}", currentMessage.Length, msg_update.Length);

            currentMessage.Append(msg_update);
            var cur_msg = currentMessage.ToString();

            // If we have no partial message yet.
            if (awaitedCloseTag == null)
            {
                // See if we have a start of message.
                var s = document_tag_name_pattern.Match(cur_msg);
                if (s.Success)
                {
                    int index = s.Groups[0].Index;

                    if (index < 0)
                    {
                        log.Error("XML Header Corrupted: Data Sent with Index:" + index.ToString() + ": length:" + cur_msg.Length + ":" + cur_msg);
                        ResetMessageBuffer();

                        // ignore data in this case until some proper data comes along.
                    }
                    else if (!cur_msg.Substring(0, index).Contains("xml version"))
                    {
                        // if XML declaration is missing then flag this up
                        log.Error("XML Header Missing: Data Sent with Index:" + index.ToString() + ": length:" + cur_msg.Length + ":" + cur_msg);
                        ResetMessageBuffer();

                        // ignore data in this case until some proper data comes along.
                    }
                    else
                    {
                        var g = s.Groups[1];
                        currentTag = g.Value;

                        if (ConfigXMLParser.ValidTag(currentTag) == false)
                        {
                            if (currentTag == null)
                            {
                                log.Error("Null Xml Tag: " + currentTag + " Fragment:" + currentMessage);
                            }
                            else
                            {
                                log.Error("Invalid Opening Xml Tag: " + currentTag + " Fragment:" + currentMessage);
                            }

                            ResetMessageBuffer();
                        }
                        else
                        {
                            awaitedCloseTag = "</" + g.Value + ">";
                            log.Info(g.Value + " found at index " + index.ToString() + ":" + g.Index.ToString());
                            BuildAndNotify(string.Empty, db, connection_id);
                        }
                    }
                }
            }
            else if (cur_msg.Contains(awaitedCloseTag))
            {
                // Find the end of the message.

                // Report the complete message.
                int end_of_current = cur_msg.IndexOf(awaitedCloseTag) + awaitedCloseTag.Length;
                int endOfCurrentWithTerminator = end_of_current;
                bool nullTerminatorFound = false;

                while (this.discardUnterminatedMessages && (endOfCurrentWithTerminator < cur_msg.Length) && (!nullTerminatorFound))
                {
                    nullTerminatorFound = (cur_msg[endOfCurrentWithTerminator] == 0);

                    if (nullTerminatorFound)
                    {
                        log.Info("Null Terminator found");
                    }

                    if (!nullTerminatorFound)
                    {
                        uint digit = (uint)cur_msg[endOfCurrentWithTerminator];
                        log.InfoFormat("Null Terminator Not found:0x{0},{3} - {1} of {2}", digit, endOfCurrentWithTerminator, cur_msg.Length, cur_msg[endOfCurrentWithTerminator]);
                    }

                    endOfCurrentWithTerminator++;
                }

                if ((!this.discardUnterminatedMessages) || nullTerminatorFound)
                {
                    if (end_of_current >= 0)
                    {
                        int msgType = GetTypeFromString(currentTag);
                        SapientMessageType msgResponse = SapientProtocol.ParseReceivedMessage(cur_msg.Substring(0, end_of_current), db, msgType);
                        SapientProtocol.SendResponse(msgResponse, connection_id);
                    }
                    else
                    {
                        log.ErrorFormat("Invalid index to end of message: {0}", end_of_current);
                        log.Error(cur_msg);
                        ResetMessageBuffer();
                    }
                }
                else
                {
                    log.ErrorFormat("Ignoring non-terminated message");
                }

                if (nullTerminatorFound)
                {
                    // Remove the reported message.
                    currentMessage.Remove(0, endOfCurrentWithTerminator);
                }
                else
                {
                    // Remove the reported message.
                    currentMessage.Remove(0, end_of_current);
                }

                // Continue processing is there is message left.
                awaitedCloseTag = null;
                currentTag = null;
                if (currentMessage.Length > 0)
                {
                    BuildAndNotify(string.Empty, db, connection_id);
                }
            }
            else if (ConfigXMLParser.ValidTag(currentTag) == false)
            {
                if (currentTag == null)
                {
                    log.Error("Null Xml Tag: " + currentTag + " Fragment:" + currentMessage);
                }
                else
                {
                    log.Error("Invalid Xml Tag: " + currentTag + " Fragment:" + currentMessage);
                    ResetMessageBuffer();
                }
            }
            else
            {
                // Wait for rest of fragment.
            }
        }

        /// <summary>
        /// Attempt recovery from a problem with parsing input messages by clearing the buffer.
        /// </summary>
        public void ResetMessageBuffer()
        {
            log.Error("Clear Message Buffer");
            awaitedCloseTag = null;
            currentTag = null;
            currentMessage.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert message type string to SapientMessageType.
        /// </summary>
        /// <param name="message">message type string.</param>
        /// <returns>Sapient Message Type.</returns>
        private static int GetTypeFromString(string message)
        {
            SapientMessageType retval = SapientMessageType.Unknown;
            log.InfoFormat("{0} received", message);

            switch (message)
            {
                case "DetectionReport":
                    retval = SapientMessageType.Detection;
                    break;

                case "StatusReport":
                    retval = SapientMessageType.Status;
                    break;

                case "SensorRegistration":
                    retval = SapientMessageType.Registration;
                    break;

                case "Alert":
                    retval = SapientMessageType.Alert;
                    break;

                case "SensorTask":
                    retval = SapientMessageType.Task;
                    break;

                case "SensorTaskACK":
                    retval = SapientMessageType.TaskACK;
                    break;

                case "SensorRegistrationACK":
                    retval = SapientMessageType.RegistrationACK;
                    break;

                case "Error":
                    retval = SapientMessageType.Error;
                    break;

                case "AlertResponse":
                    retval = SapientMessageType.AlertResponse;
                    break;

                case "Objective":
                    retval = SapientMessageType.Objective;
                    break;

                case "RoutePlan":
                    retval = SapientMessageType.RoutePlan;
                    break;

                case "Approval":
                    retval = SapientMessageType.Approval;
                    break;
            }

            return (int)retval;
        }

        #endregion
    }
}