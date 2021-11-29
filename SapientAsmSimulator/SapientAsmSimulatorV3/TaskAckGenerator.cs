// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskAckGenerator.cs$
// <copyright file="TaskAckGenerator.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.IO;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Task Acknowledgement messages
    /// </summary>
    public class TaskAckGenerator
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// sends out the task ACK message 
        /// </summary>
        /// <param name="messenger">object used to send messages over</param>
        /// <param name="form">main form</param>
        /// <param name="task">task object</param>
        /// <param name="logger">file data logger</param> 
        public static void GenerateTaskAck(IConnection messenger, IGUIInterface form, SensorTask task, SapientLogger logger)
        {
            string xmlstring = GenerateXml(task);
            bool retval = MessageSender.Send(messenger, xmlstring, logger);

            if (retval)
            {
                form.UpdateOutputWindow("SensorTaskACK Sent");
                Log.InfoFormat("Send SensorTaskACK {0} Succeeded", task.taskID);
            }
            else
            {
                form.UpdateOutputWindow("SensorTaskACK Send failed");
                Log.ErrorFormat("Send SensorTaskACK {0} Failed", task.taskID);
            }
        }

        /// <summary>
        /// Generate XML message string
        /// </summary>
        /// <param name="task">Source Sensor Task Object</param>
        /// <returns>XML string</returns>
        private static string GenerateXml(SensorTask task)
        {
            SensorTaskACK sensor_task_ack = new SensorTaskACK
            {
                sensorID = task.sensorID,
                taskID = task.taskID,
                timestamp = DateTime.UtcNow,
                status = "Accepted",
            };

            sensor_task_ack.associatedFile = new[]
            {
                new SensorTaskACKAssociatedFile { type = "image", url = "filenameYYMMDD_HHMMSS.jpg" },
            };

            string xmlstring = ConfigXMLParser.Serialize(sensor_task_ack);
            File.WriteAllLines(@".\SensorTaskACK1.xml", new[] { xmlstring });
            return xmlstring;
        }
    }
}
