// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskACKParser.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using log4net;
    using SapientServices.Data;
    using System;

    /// <summary>
    /// Parser for Task Acknowledgement messages
    /// </summary>
    public class TaskACKParser
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void ParseTaskACK(string message, TaskForm form)
        {
            int messageCount = 0;
            int offset = 0;
            const int closeTagLength = 16;

            do
            {
                int index = message.IndexOf("</SensorTaskACK>", offset);

                if (index > 0)
                {
                    Log.InfoFormat("SensorTaskACK at:{0}", index);

                    if (index + closeTagLength > message.Length)
                    {
                        Log.ErrorFormat("ParseTaskACK Error offset {0} length {1}", index + closeTagLength, message.Length);

                        // error so exit loop
                        offset = message.Length;
                    }
                    else
                    {
                        string singleMessage = message.Substring(offset, index - offset + closeTagLength);
                        {
                            var id = (SensorTaskACK)ConfigXMLParser.Deserialize(typeof(SensorTaskACK), singleMessage);
                            messageCount++;

                            // Added to pass SAPIENT_Test_Harness_Build_Note-O
                            form.UpdateOutputWindow("SensorTaskACK: " + message + "\n");
                            form.UpdateOutputWindow("Latency(ms):  " + (DateTime.UtcNow - id.timestamp).TotalMilliseconds);
                            Log.InfoFormat("{0}:SensorTaskACK Task ID: {1}:{2}:{3}", messageCount, id.taskID, id.status, id.reason);
                        }

                        // iterate on to next message
                        offset += (index + closeTagLength);

                        while ((offset < message.Length) && ((message[offset] == 0) || (message[offset] == ' ') || (message[offset] == '\r') || (message[offset] == '\n')))
                        {
                            offset++;
                        }
                    }
                }
                else
                {
                    // not found so exit loop
                    offset = message.Length;
                }
            } while (offset < message.Length);
        }
    }
}
