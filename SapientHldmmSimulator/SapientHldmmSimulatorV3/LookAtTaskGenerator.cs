// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: LookAtTaskGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

using System;
using System.Threading;
using log4net;
using SapientServices.Communication;
using SapientServices.Data;

namespace SapientHldmmSimulator
{
    /// <summary>
    /// Generate Look At Sensor Task messages
    /// </summary>
    public class LookAtTaskGenerator : TaskGenerator
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Generate and send command to one or more sensors
        /// </summary>
        /// <param name="az">azimuth in degrees</param>
        /// <param name="range">range in meters</param>
        /// <param name="ptz">raw PTZ command</param>
        /// <param name="zoom">zoom value</param> 
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="baseSensorID">base sensor Identifier</param>
        /// <param name="numSensors">Number of sensors</param>
        public void GeneratePTZTask(double az, double ele, bool ptz, double zoom, object comms_connection, int baseSensorID, int numSensors)
        {
            IConnection messenger = (IConnection)comms_connection;
            do
            {
                SendTaskToMultipleSensors(az, ele, ptz, zoom, comms_connection, baseSensorID, taskId, numSensors);

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
        /// <param name="az">azimuth in degrees</param>
        /// <param name="range">range in meters</param>
        /// <param name="ptz">raw PTZ command</param>
        /// <param name="zoom">zoom value</param> 
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="baseSensorID">base sensor Identifier</param>
        /// <param name="taskId"task ID>task Identifier</param>
        /// <param name="numSensors">Number of sensors</param>
        private static void SendTaskToMultipleSensors(double az, double ele, bool ptz, double zoom, object comms_connection, int baseSensorID, int taskId, int numSensors)
        {
            var messenger = (IConnection)comms_connection;

            int sensorNum;
            for (sensorNum = 0; sensorNum < numSensors; sensorNum++)
            {
                int sensorId = sensorNum + baseSensorID;
                SensorTask sensorTask = GeneratePTZTask(az, ele, ptz, zoom, sensorId, taskId);

                string xmlstring = ConfigXMLParser.Serialize(sensorTask);
                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    Log.InfoFormat("Sent LookAtCommand:{0} to SensorID:{1}", taskId, sensorId);
                }
                else
                {
                    Log.InfoFormat("Sent LookAtCommand:{0} to SensorID:{1} FAILED", taskId, sensorId);
                }
            }
        }

        /// <summary>
        /// method to generate the PTZ sensor task message
        /// </summary>
        /// <param name="az">azimuth in degrees</param>
        /// <param name="range">range in meters</param>
        /// <param name="ptz">raw PTZ command</param>
        /// <param name="zoom">zoom value</param>
        /// <returns>sensor task message</returns>
        private static SensorTask GeneratePTZTask(double az, double ele, bool ptz, double zoom, int sensorId, int taskId)
        {
            var sensor_task = new SensorTask
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

            if (ptz)
            {
                sensor_task.command.lookAt = new SensorTaskCommandLookAt
                {
                    rangeBearingCone = new rangeBearingCone
                    {
                        Az = az,
                        Ele = ele,
                        EleSpecified = true,
                        R = 0,
                        hExtent = zoom,
                        vExtent = 0.0,
                        vExtentSpecified = true
                    }
                };
            }
            else
            {
                // now use zoom value for horizontal field of view
                sensor_task.command.lookAt = new SensorTaskCommandLookAt
                {
                    rangeBearingCone = new rangeBearingCone
                    {
                        Az = az,
                        Ele = ele,
                        EleSpecified = true,
                        R = zoom,
                        hExtent = 0,
                    }
                };
            }

            return sensor_task;
        }
    }
}
