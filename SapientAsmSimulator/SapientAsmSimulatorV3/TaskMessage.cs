// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskMessage.cs$
// <copyright file="TaskMessage.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Handle Task Messages
    /// </summary>
    public class TaskMessage
    {
        /// <summary> The logger </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskMessage));

        /// <summary>
        /// Gets or sets the sensor identifier to use in acknowledgement messages
        /// </summary>
        public static int AsmId { get; set; }

        /// <summary>
        /// Gets or sets a reference to heartbeat generator class for request heartbeat task message
        /// </summary>
        public static HeartbeatGenerator HeartbeatGenerator { get; set; }

        /// <summary>
        /// Process incoming sensor task message to check its well formed and camera can process
        /// </summary>
        /// <param name="message">message to parse</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">main form</param>
        /// <param name="logger">logger object</param>
        public static void ProcessSensorTaskMessage(string message, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            Log.Info("Task Message Received");
            SensorTask task = (SensorTask)ConfigXMLParser.Deserialize(typeof(SensorTask), message);

            if (task.command != null)
            {
                if (string.IsNullOrEmpty(task.command.request) == false)
                {
                    ProcessCommandRequestMessage(task, messenger, form, logger);
                }
                else if (string.IsNullOrEmpty(task.command.detectionThreshold) == false)
                {
                    ProcessDetectionThresholdMessage(task, messenger, form, logger);
                }
                else if (string.IsNullOrEmpty(task.command.detectionReportRate) == false)
                {
                    ProcessDetectionReportRateMessage(task, messenger, form, logger);
                }
                else if (string.IsNullOrEmpty(task.command.mode) == false)
                {
                    ProcessModeMessage(task, messenger, form, logger);
                }
                else if (task.command.lookAt != null)
                {
                    ProcessLookAtMessage(task, messenger, form, logger);
                }
                else
                {
                    NotSupported(task.taskID, messenger, form, logger);
                }
            }
            else if (task.region != null)
            {
                ProcessRegionMessage(task, messenger, form, logger);
            }
            else
            {
                SendTaskAck(task.taskID, "Rejected", "Task Error", messenger, form, logger);
            }
        }

        /// <summary>
        /// Send a task complete acknowledgement
        /// </summary>
        /// <param name="taskId">task id to send</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        public static void SendComplete(int taskId, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            SendTaskAck(taskId, "Complete", string.Empty, messenger, form, logger);
        }

        /// <summary>
        /// Report that a task command is not currently supported by the PTZ ASM
        /// </summary>
        /// <param name="taskId">task id to send</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void NotSupported(int taskId, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            SendTaskAck(taskId, "Rejected", "Not Supported", messenger, form, logger);
        }

        /// <summary>
        /// Send Task Acknowledgement Message
        /// </summary>
        /// <param name="taskId">task id to send</param>
        /// <param name="ackStatus">status field</param>
        /// <param name="ackReason">reason field</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void SendTaskAck(int taskId, string ackStatus, string ackReason, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            var ack = new SensorTaskACK
            {
                timestamp = DateTime.UtcNow,
                sensorID = TaskMessage.AsmId,
                status = ackStatus,
                reason = ackReason,
                taskID = taskId
            };

            var xmlstring = ConfigXMLParser.Serialize(ack);
            bool retval = MessageSender.Send(messenger, xmlstring, logger);
        }

        /// <summary>
        /// Handle sensor task messages that include command request fields
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessCommandRequestMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            if (string.IsNullOrEmpty(task.command.request) == false)
            {
                if (task.command.request == "Registration")
                {
                    RegistrationGenerator.GenerateRegistration(messenger, form, logger);
                    TaskAckGenerator.GenerateTaskAck(messenger, form, task, logger);
                }
                else if (task.command.request == "Reset")
                {
                    //// TODO
                }
                else if (task.command.request == "Heartbeat")
                {
                    HeartbeatGenerator.GenerateStatus(messenger, logger);
                }
                else if (task.command.request == "Stop")
                {
                    //// TODO
                }
                else if (task.command.request == "Start")
                {
                    //// TODO
                }
                else if (task.command.request == "Take Snapshot")
                {
                    TaskAckGenerator.GenerateTaskAck(messenger, form, task, logger);
                }
                else
                {
                    NotSupported(task.taskID, messenger, form, logger);
                }
            }
        }

        /// <summary>
        /// Handle sensor task messages that include Detection Threshold fields
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessDetectionThresholdMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            if (task.command.detectionThreshold == "Low")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else if (task.command.detectionThreshold == "Medium")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else if (task.command.detectionThreshold == "High")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else
            {
                NotSupported(task.taskID, messenger, form, logger);
            }
        }

        /// <summary>
        /// Handle sensor task messages that include DetectionReportRate fields
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessDetectionReportRateMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            if (task.command.detectionThreshold == "Low")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else if (task.command.detectionThreshold == "Medium")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else if (task.command.detectionThreshold == "High")
            {
                SendComplete(task.taskID, messenger, form, logger);
            }
            else
            {
                NotSupported(task.taskID, messenger, form, logger);
            }
        }

        /// <summary>
        /// Handle sensor task messages that include command mode fields
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessModeMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            {
                NotSupported(task.taskID, messenger, form, logger);
            }
        }

        /// <summary>
        /// Handle sensor task messages that include lookAt command
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessLookAtMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            if (task.command.lookAt.rangeBearingCone != null)
            {
                bool isNewDefaultTask = false; // non-persistent task
                ProcessRegion(task.command.lookAt.rangeBearingCone, task, isNewDefaultTask);
            }
            else
            {
                // location list not supported
                NotSupported(task.taskID, messenger, form, logger);
            }
        }

        /// <summary>
        /// Handle sensor task messages that specify a region of interest
        /// </summary>
        /// <param name="task">task message object</param>
        /// <param name="messenger">connection object</param>
        /// <param name="form">GUI form</param>
        /// <param name="logger">logger object</param>
        private static void ProcessRegionMessage(SensorTask task, IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            if (task.region[0] != null)
            {
                if (task.region[0].rangeBearingCone != null)
                {
                    bool isNewDefaultTask = true; // make this task persist
                    ProcessRegion(task.region[0].rangeBearingCone, task, isNewDefaultTask);
                }
                else
                {
                    // location list not supported
                    NotSupported(task.taskID, messenger, form, logger);
                }
            }
        }

        /// <summary>
        /// Handle Region of Interest Type commands
        /// </summary>
        /// <param name="rb">region of interest</param>
        /// <param name="task">original task message</param>
        /// <param name="isNewDefaultTask">whether the task should be the default task</param>
        private static void ProcessRegion(rangeBearingCone rb, SensorTask task, bool isNewDefaultTask)
        {
            DateTime timenow = DateTime.UtcNow;

            if (rb.hExtent != 0)
            {
                HeartbeatGenerator.HExtent = rb.hExtent;
            }

            try
            {
                if (rb.R == 0)
                {
                    // PTZ task
                    if (rb.vExtentSpecified)
                    {
                        Log.InfoFormat("PTZ Task:P:{0}, T:{1}, Z:{2}", rb.Az, rb.Ele, rb.hExtent);
                        HeartbeatGenerator.Range = rb.R;
                        HeartbeatGenerator.Azimuth = rb.Az;
                        HeartbeatGenerator.Elevation = rb.Ele;
                    }
                    else
                    {
                        Log.InfoFormat("PTFOV Task:{0},t:{1},n:{2}, P:{3}, T:{4}, HFOV:{5}", task.taskID, task.timestamp, timenow, rb.Az, rb.Ele, rb.hExtent);
                        HeartbeatGenerator.Range = rb.R;
                        HeartbeatGenerator.Azimuth = rb.Az;
                        HeartbeatGenerator.Elevation = rb.Ele;
                    }
                }
                else
                {
                    Log.InfoFormat("Task:{0},t:{1},n:{2}, Az:{3:F1} El:{4:F1} R:{5:F1} FOV:{6:F1}", task.taskID, task.timestamp, timenow, rb.Az, rb.Ele, rb.R, rb.hExtent);
                    HeartbeatGenerator.Range = rb.R;
                    HeartbeatGenerator.Azimuth = rb.Az;
                    HeartbeatGenerator.Elevation = rb.Ele;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in region task:", ex);
            }
        }
    }
}
