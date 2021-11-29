// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

using System;
using System.Threading;
using log4net;
using SapientServices.Communication;
using SapientServices.Data;

namespace SapientHldmmSimulator
{
    /// <summary>
    /// Generate Sensor Task messages
    /// </summary>
    public class TaskGenerator : XmlGenerators
    {
        #region Data Members

        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// task identifier
        /// </summary>
        protected int taskId = 1;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskGenerator()
        {
            ChangeTaskID = true;
        }

        #region Properties
        public static bool SendToHldmm { get; set; }

        public bool ChangeTaskID { get; set; }

        public string CommandString { get; set; }

        public int BaseSensorID { get; set; }

        public int NumSensors { get; set; }

        #endregion

        /// <summary>
        /// Generate and send command to one or more sensors
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        public void GenerateCommand(object comms_connection)
        {
            IConnection messenger = (IConnection)comms_connection;
            do
            {
                SendTaskToMultipleSensors(comms_connection, CommandString, BaseSensorID, taskId, NumSensors);

                if (ChangeTaskID)
                {
                    taskId++;
                }

                if (LoopMessages) Thread.Sleep(LoopTime);
            } while (LoopMessages);
        }

        /// <summary>
        /// Send Task to one or more sensors
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="commandString">command string</param>
        /// <param name="baseSensorID">base sensor Identifier</param>
        /// <param name="taskId"task ID>task Identifier</param>
        /// <param name="numSensors">Number of sensors</param>
        private static void SendTaskToMultipleSensors(object comms_connection, string commandString, int baseSensorID, int taskId, int numSensors)
        {
            var messenger = (IConnection)comms_connection;

            int sensorNum;
            for (sensorNum = 0; sensorNum < numSensors; sensorNum++)
            {
                int sensorId = sensorNum + baseSensorID;
                SensorTask sensorTask = GenerateCommand(commandString, sensorId, taskId);

                string xmlstring = ConfigXMLParser.Serialize(sensorTask);
                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    Log.InfoFormat("Sent Command:{0} to SensorID:{1}", taskId, sensorId);
                }
                else
                {
                    Log.InfoFormat("Sent Command:{0} to SensorID:{1} FAILED", taskId, sensorId);
                }
            }
        }

        /// <summary>
        /// Generate Command Tasks . NB LookAt is generated elsewhere as it is more complex
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        private static SensorTask GenerateCommand(string commandString, int sensorId, int taskId)
        {
            SensorTask sensor_task = null;

            if (commandString.Contains("request "))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.request = commandString.Substring(8); // strip 'request' and whitespace;
            }
            else if (commandString.Contains("detectionThreshold "))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.detectionThreshold = commandString.Substring(19); // strip 'detectionThreshold' and whitespace;
            }
            else if (commandString.Contains("detectionReportRate "))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.detectionThreshold = commandString.Substring(20); // strip 'detectionReportRate' and whitespace;
            }
            else if (commandString.Contains("mode "))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.mode = commandString.Substring(5); // strip 'mode' and whitespace;
            }
            else if (commandString.Contains("Take Control"))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.request = "Take Control";
                sensor_task.taskName = commandString;
            }
            else if (commandString.Contains("Release Control"))
            {
                sensor_task = CreateDefaultSensorTask(sensorId, taskId);
                sensor_task.command.request = "Release Control";
                sensor_task.taskName = commandString;
            }

            return sensor_task;
        }

        /// <summary>
        /// Generate default sensor task object
        /// </summary>
        /// <param name="sensorId"></param>
        /// <returns></returns>
        private static SensorTask CreateDefaultSensorTask(int sensorId, int taskId)
        {
            SensorTask sensor_task = new SensorTask
            {
                sensorID = sensorId,
                taskID = taskId,
                timestamp = DateTime.UtcNow,
                control = "Start",
            };

            sensor_task.command = new SensorTaskCommand();

            if (SendToHldmm)
            {
                sensor_task.taskName = "Manual Task";
                sensor_task.sensorID = 0;
                sensor_task.command.sensorID = sensorId;
                sensor_task.command.sensorIDSpecified = true;
            }

            return sensor_task;
        }
    }
}
