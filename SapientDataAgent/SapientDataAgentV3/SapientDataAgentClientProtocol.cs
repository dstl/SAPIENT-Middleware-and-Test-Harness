// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientDataAgentClientProtocol.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using log4net;
    using SapientDatabase;
    using SapientServices;
    using SapientServices.Data;

    /// <summary>
    /// class to handle the Sapient Data Data Agent Client Protocol.
    /// </summary>
    public class SapientDataAgentClientProtocol : SapientProtocol
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly new ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// sensor locations.
        /// </summary>
        private Dictionary<long, location> sensorLocations = new Dictionary<long, location>();

        /// <summary>
        /// report ID of last detection received.
        /// </summary>
        private long lastDetectionReportID;

        /// <summary>
        /// report ID of last status report received.
        /// </summary>
        private long lastStatusReportID;

        /// <summary>
        /// time histogram of database latency.
        /// </summary>
        private Histogram databaseLatencyHistogram;

        /// <summary>
        /// time histogram of communication latency.
        /// </summary>
        private Histogram communicationLatencyHistogram;

        /// <summary>
        /// whether to forward ASM alerts to HLDMM.
        /// </summary>
        private bool forwardAsmAlerts;

        /// <summary>
        /// Fixed asm id specified in the config file.
        /// </summary>
        private int fixedAsmId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientDataAgentClientProtocol" /> class.
        /// </summary>
        /// <param name="forwardASMalerts">whether to forward ASM alerts to HLDMM.</param>
        /// <param name="fixedASMId">fixed ASM ID from config file.</param>
        public SapientDataAgentClientProtocol(bool forwardASMalerts, int fixedASMId)
        {
            forwardAsmAlerts = forwardASMalerts;
            fixedAsmId = fixedASMId;
            databaseLatencyHistogram = new Histogram();
            communicationLatencyHistogram = new Histogram();

            // Specify the messages supported by this connection
            Supported.Add(SapientMessageType.Registration);
            Supported.Add(SapientMessageType.Detection);
            Supported.Add(SapientMessageType.Status);
            Supported.Add(SapientMessageType.Alert);
            Supported.Add(SapientMessageType.TaskACK);
            Supported.Add(SapientMessageType.RoutePlan);
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
                    var send = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(send, Program.TaskingCommsConnection, "Sensor Registration");
                    }
                    else
                    {
                        Log.Warn("HLDMM data agent not connected");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Registration);
                    break;

                case SapientMessageType.TaskACK:
                    var sensorAck = Encoding.UTF8.GetBytes(TaskMessage);
                    if (Program.TaskingCommsConnection != null)
                    {
                        ForwardMessage(sensorAck, Program.TaskingCommsConnection, "Sensor Task Ack");
                    }
                    else
                    {
                        Log.Warn("HLDMM data agent not connected");
                    }

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.TaskACK);
                    break;

                case SapientMessageType.Unknown:
                    Log.ErrorFormat("Unrecognised Message received from ASM: {0}", TaskMessage);
                    var errorMessage = new Error
                    {
                        timestamp = DateTime.UtcNow,
                        packet = TaskMessage,
                        errorMessage = "Unrecognised message received",
                    };
                    var error = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(errorMessage));
                    ForwardMessage(error, Program.ClientComms, connection, "Unrecognised Message - Error returned: " + TaskMessage, "Unrecognised message - Error return failed: " + TaskMessage);

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                case SapientMessageType.InvalidClient:
                    Log.ErrorFormat("Invalid Message received:{0}:{1}", this.ErrorString, TaskMessage);
                    errorMessage = new Error
                    {
                        timestamp = DateTime.UtcNow,
                        packet = TaskMessage,
                        errorMessage = ErrorString,
                    };
                    error = Encoding.UTF8.GetBytes(ConfigXMLParser.Serialize(errorMessage));
                    ForwardMessage(error, Program.ClientComms, connection, "Invalid Message :" + this.ErrorString + ": Error returned: " + TaskMessage, "Invalid message - Error return failed: " + TaskMessage);

                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InvalidClient);
                    break;

                case SapientMessageType.Detection:
                case SapientMessageType.Status:
                case SapientMessageType.RoutePlan:
                    Program.MessageMonitor.IncrementMessageCount(output);
                    break;

                case SapientMessageType.Alert:
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Alert);
                    if (forwardAsmAlerts)
                    {
                        if (Program.TaskingCommsConnection != null)
                        {
                            var alertf = Encoding.UTF8.GetBytes(TaskMessage);
                            ForwardMessage(alertf, Program.TaskingCommsConnection, "Alert");
                        }
                        else
                        {
                            Log.Warn("HLDMM data agent not connected");
                        }
                    }

                    break;

                case SapientMessageType.IdError:
                    Log.ErrorFormat("Message with unregistered ASM ID received from ASM: {0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.IdError);
                    break;

                case SapientMessageType.InternalError:
                    Log.ErrorFormat("SDA Client Error:{0}", ErrorString);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.InternalError);
                    break;

                case SapientMessageType.Unsupported:
                    Log.ErrorFormat("Unsupported Message received from ASM: {0}", TaskMessage);
                    Program.MessageMonitor.IncrementMessageCount(SapientMessageType.Unknown);
                    break;

                default:
                    Log.ErrorFormat("Unknown Middleware Response:{0}", output);
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
                    Program.MessageMonitor.SetStatusText(0, registration.sensorType);
                    Program.Registration = registration;
                    if (fixedAsmId > 0)
                    {
                        // enforce fixed sensor ID by always registering with the same Identifier
                        registration.sensorID = fixedAsmId;
                        registration.sensorIDSpecified = true;
                        TaskMessage = ConfigXMLParser.Serialize(registration);
                    }

                    // Log registered ASMs for recovery of dropped connections
                    LogRegisteredSensor(registration.sensorID);
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
        /// Parses and processes a detection report message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessDetectionReport(string message, Database database)
        {
            Log.Info("ASM Detection Message Received:");
            try
            {
                DetectionReport detectionReport;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseDetection(message, out detectionReport, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Detection)
                {
                    var sourceId = detectionReport.sourceID;

                    // If the sensor is not currently registered with us, check if it has been in
                    // the past and assume it is the same sensor having recovered from a network drop.
                    bool found = CurrentOrPreviousASM(sourceId);

                    if ((lastDetectionReportID + 1) != detectionReport.reportID)
                    {
                        Log.InfoFormat("Non Consecutive Detection ReportID {0} {1}", sourceId, detectionReport.reportID);
                    }

                    Log.InfoFormat("source:{0} det reportID:{1}", sourceId, detectionReport.reportID);
                    lastDetectionReportID = detectionReport.reportID;
                    if (found)
                    {
                        // Update last contact from sensor.
                        LogRegisteredSensor(sourceId);

                        // start stop watch for timing diagnostics.
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        if (detectionReport.location != null) detectionReport.location = CartesianOffset(detectionReport.location, detectionReport.sourceID, database);
                        if (detectionReport.rangeBearing != null) detectionReport.rangeBearing = BearingOffset(detectionReport.rangeBearing, detectionReport.sourceID, database);

                        // Write to standard database.
                        database.DbDetection(detectionReport, false);

                        if (AdditionalDatabase != null)
                        {
                            AdditionalDatabase.DbDetection(detectionReport, false);
                        }

                        // measure communications and database latency.
                        long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                        databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                        double latency = (DateTime.UtcNow - detectionReport.timestamp).TotalMilliseconds;
                        communicationLatencyHistogram.Add(latency);

                        Log.Info("DetectionReport Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                        Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());

                        // Don't update latency based on detection reports as these are dependent on original data so there is additional latency to just comms.
                        Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                    }
                    else
                    {
                        ErrorString = string.Format("ASM ID: {0} not registered", sourceId);
                        retval = SapientMessageType.IdError;
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:DetectionReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes a status report message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessStatusReport(string message, Database database)
        {
            Log.Info("Status Report Message Received");

            try
            {
                StatusReport statusReport;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseStatusReport(message, out statusReport, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.Status)
                {
                    var sourceId = statusReport.sourceID;

                    // If the sensor is not currently registered with us, check if it has been in
                    // the past and assume it is the same sensor having recovered from a network drop.
                    bool found = CurrentOrPreviousASM(sourceId);

                    if ((lastStatusReportID + 1) != statusReport.reportID)
                    {
                        Log.InfoFormat("Non Consecutive Status ReportID {0} {1}", sourceId, statusReport.reportID);
                    }

                    Log.InfoFormat("source:{0} sta reportID:{1}", sourceId, statusReport.reportID);
                    lastStatusReportID = statusReport.reportID;
                    if (statusReport.sensorLocation.location != null) statusReport.sensorLocation.location = CartesianOffset(statusReport.sensorLocation.location, statusReport.sourceID, database);
                    if (statusReport.fieldOfView.rangeBearingCone != null) statusReport.fieldOfView.rangeBearingCone = BearingOffset(statusReport.fieldOfView.rangeBearingCone, statusReport.sourceID, database);
                    if (statusReport.fieldOfView.locationList != null) statusReport.fieldOfView.locationList = CartesianOffset(statusReport.fieldOfView.locationList, statusReport.sourceID, database);
                    if (found)
                    {
                        // Update last contact from sensor.
                        LogRegisteredSensor(sourceId);

                        string sensorStatus = statusReport.system;
                        Program.MessageMonitor.SetStatusText(1, sensorStatus);

                        // start stop watch for timing diagnostics.
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        if ((statusReport.fieldOfView != null) && (statusReport.fieldOfView.rangeBearingCone != null))
                        {
                            double azOffset = Properties.Settings.Default.AzimuthOffset;

                            double fixedRange = Properties.Settings.Default.FixedRange;

                            Log.Info(" StatusReport Az:" + statusReport.fieldOfView.rangeBearingCone.Az.ToString() + " AzOff:" + azOffset.ToString());

                            if (azOffset != 0)
                            {
                                statusReport.fieldOfView.rangeBearingCone.Az += azOffset;
                                Log.Info(" StatusReport Applied Azo:" + statusReport.fieldOfView.rangeBearingCone.Az.ToString());
                            }

                            if (fixedRange > 0)
                            {
                                statusReport.fieldOfView.rangeBearingCone.R = fixedRange;
                                Log.Info(" StatusReport Fixed Range:" + fixedRange.ToString());
                            }
                        }

                        // Update sensorLocation dictionary to hold sensor positions.
                        if (statusReport.sensorLocation != null)
                        {
                            sensorLocations[statusReport.sourceID] = statusReport.sensorLocation.location;
                        }

                        // Write data to database.
                        database.DbStatus(statusReport);

                        if (AdditionalDatabase != null)
                        {
                            AdditionalDatabase.DbStatus(statusReport);
                        }

                        // measure communications and database latency.
                        long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                        databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                        double latency = (DateTime.UtcNow - statusReport.timestamp).TotalMilliseconds;
                        communicationLatencyHistogram.Add(latency);

                        Log.Info("StatusReport Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                        Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                        Program.MessageMonitor.SetLatency(0, latency);
                        Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                    }
                    else
                    {
                        ErrorString = string.Format("ASM ID: {0} not registered", sourceId);
                        retval = SapientMessageType.IdError;
                    }
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:StatusReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parses and processes an alert message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
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

                    // If the sensor is not currently registered with us
                    // check if it has been in the past and assume it is the same sensor having recovered from a network drop.
                    bool found = CurrentOrPreviousASM(sourceId);
                    if (alert.location != null) alert.location = CartesianOffset(alert.location, alert.sourceID, database);
                    if (alert.rangeBearing != null) alert.rangeBearing = BearingOffset(alert.rangeBearing, alert.sourceID, database);
                    if (found)
                    {
                        // Update last contact from sensor.
                        LogRegisteredSensor(sourceId);

                        // start stop watch for timing diagnostics.
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        // write data to database.
                        database.DbAlert(alert, false);

                        // measure communications and database latency.
                        long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                        databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                        double latency = (DateTime.UtcNow - alert.timestamp).TotalMilliseconds;
                        communicationLatencyHistogram.Add(latency);

                        Log.Info("Alert Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                        Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                        Program.MessageMonitor.SetLatency(0, latency);
                        Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                    }
                    else
                    {
                        ErrorString = string.Format("ASM ID: {0} not registered", sourceId);
                        retval = SapientMessageType.IdError;
                    }
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
        /// Parses and processes a sensor task ack message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="database">The database connection.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessSensorTaskAck(string message, Database database)
        {
            Log.Info("Sensor Task Ack Message Received");
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
                    if ((sensorTaskAck.associatedFile != null) && (sensorTaskAck.associatedFile[0].url != null))
                    {
                        int fileCount = sensorTaskAck.associatedFile.Length;
                        int index;
                        for (index = 0; index < fileCount; index++)
                        {
                            // strip leading slash.
                            if (sensorTaskAck.associatedFile[index].url.StartsWith("/"))
                            {
                                sensorTaskAck.associatedFile[index].url = sensorTaskAck.associatedFile[index].url.Substring(1);
                            }

                            // add sensor folder.
                            if (!sensorTaskAck.associatedFile[index].url.StartsWith("sensor_"))
                            {
                                sensorTaskAck.associatedFile[index].url = string.Format("sensor_{0}/{1}", sensorTaskAck.sensorID, sensorTaskAck.associatedFile[index].url);
                            }
                        }
                    }

                    // write sensor_task_ack to database.
                    database.DbAcknowledgeTask(sensorTaskAck, SapientDatabase.DatabaseTables.TaskConstants.SensorTaskAck.Table);

                    // measure database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    stopWatch.Stop();
                    Log.Info("taskAck Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms");
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:SensorTaskACK:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT RoutePlan Message and return success or otherwise.
        /// </summary>
        /// <param name="message">message string.</param>
        /// <param name="database">database to write to.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        protected override SapientMessageType ProcessRoutePlan(string message, Database database)
        {
            Log.Info("Route Plan Message Received");
            Log.Info(message);

            try
            {
                RoutePlan routePlan;
                string errorString;
                SapientMessageType retval = SapientMessageValidator.ParseRoutePlan(message, out routePlan, out errorString);
                ErrorString = errorString;

                if (retval == SapientMessageType.RoutePlan)
                {
                    // start stop watch for timing diagnostics.
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // write data to database.
                    database.DbRoutePlan(routePlan);

                    // measure communications and database latency.
                    long databaseLatencyMilliseconds = stopWatch.ElapsedMilliseconds;
                    databaseLatencyHistogram.Add(databaseLatencyMilliseconds);

                    double latency = (DateTime.UtcNow - routePlan.timestamp).TotalMilliseconds;
                    communicationLatencyHistogram.Add(latency);

                    Log.Info("RoutePlan Message Latency, " + latency.ToString("F1") + ", ms, " + communicationLatencyHistogram.Print());
                    Log.Info("Database Latency, " + databaseLatencyMilliseconds.ToString("D") + ", ms, " + databaseLatencyHistogram.Print());
                    Program.MessageMonitor.SetLatency(0, latency);
                    Program.MessageMonitor.SetLatency(1, databaseLatencyMilliseconds);
                }

                return retval;
            }
            catch (Exception e)
            {
                ErrorString = "Internal:RoutePlan:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Checks given id against a list of previously registered sensors to see if the current
        /// sensor has registered with this SDA within a time frame provided in the config.
        /// </summary>
        /// <param name="sensorId">The ID of an ASM to verify.</param>
        /// <returns>Has the sensor previously registered with this SDA within the configured time limit.</returns>
        private bool HasRegisteredPreviously(int sensorId)
        {
            bool found = false;

            if (Program.previouslyConnectedASMs.ContainsKey(sensorId))
            {
                if ((DateTime.Now - Program.previouslyConnectedASMs[sensorId]) < Properties.Settings.Default.AsmReconnectionTimeout)
                {
                    found = true;
                }
            }

            return found;
        }

        /// <summary>
        /// whether this is a current or previously registered ASM ID.
        /// </summary>
        /// <param name="source_id">ASM or sensor ID.</param>
        /// <returns>true if currently registered.</returns>
        private bool CurrentOrPreviousASM(int source_id)
        {
            bool found = false;
            lock (Program.ClientMessageParsers)
            {
                found = SensorIds.Contains(source_id);
            }

            // If the sensor is not currently registered with us, check if it has been in
            // the past and assume it is the same sensor having recovered from a network drop.
            if (!found)
            {
                found = HasRegisteredPreviously(source_id);
            }

            return found;
        }

        /// <summary>
        /// Adds (or updates) a sensor in a collection with the current DateTime. This is used for reconnecting ASMs which have dropped connection.
        /// </summary>
        /// <param name="sensorId">ASM or sensor ID.</param>
        private void LogRegisteredSensor(int sensorId)
        {
            Program.previouslyConnectedASMs.AddOrUpdate(sensorId, DateTime.Now, (key, previousValue) => DateTime.Now);
        }

        /// <summary>
        /// Offset a location in cartesian coordinates.
        /// </summary>
        /// <param name="location">location object to apply offset to.</param>
        /// <param name="sensor_id">ASM or sensor ID.</param>
        /// <param name="database">offset database object.</param>
        /// <returns>location object with offset applied.</returns>
        private location CartesianOffset(location location, long sensor_id, Database database)
        {
            Dictionary<long, Dictionary<string, double>> locationOffset = database.GetSensorOffsetFromDB(sensor_id);
            if (locationOffset.ContainsKey(sensor_id))
            {
                location.X += locationOffset[sensor_id]["x_offset"];
                location.Y += locationOffset[sensor_id]["y_offset"];
                location.Z += locationOffset[sensor_id]["z_offset"];
            }

            return location;
        }

        /// <summary>
        /// offset a set of locations in cartesian.
        /// </summary>
        /// <param name="locations">list of locations to apply offset to.</param>
        /// <param name="sensor_id">ASM or sensor ID.</param>
        /// <param name="database">offset database object.</param>
        /// <returns>list of locations with offset applied.</returns>
        private locationList CartesianOffset(locationList locations, long sensor_id, Database database)
        {
            for (int i = 0; i < locations.location.Length; i++)
            {
                locations.location[i] = CartesianOffset(locations.location[i], sensor_id, database);
            }

            return locations;
        }

        /// <summary>
        /// offset a bearing location in azimuth and elevation angles.
        /// </summary>
        /// <param name="bearing">range bearing object to apply offset to.</param>
        /// <param name="sensor_id">sensor ID.</param>
        /// <param name="database">database object.</param>
        /// <returns>range bearing object with offset applied.</returns>
        private rangeBearing BearingOffset(rangeBearing bearing, long sensor_id, Database database)
        {
            Dictionary<long, Dictionary<string, double>> locationOffset = database.GetSensorOffsetFromDB(sensor_id);
            if (locationOffset.ContainsKey(sensor_id))
            {
                bearing.Az += locationOffset[sensor_id]["az_offset"];
                bearing.Ele += locationOffset[sensor_id]["ele_offset"];
            }

            return bearing;
        }

        /// <summary>
        /// offset a bearing cone location in azimuth and elevation angles.
        /// </summary>
        /// <param name="bearing">range bearing cone object to apply offset to.</param>
        /// <param name="sensor_id">sensor ID.</param>
        /// <param name="database">database object.</param>
        /// <returns>>range bearing cone object with offset applied.</returns>
        private rangeBearingCone BearingOffset(rangeBearingCone bearing, long sensor_id, Database database)
        {
            Dictionary<long, Dictionary<string, double>> locationOffset = database.GetSensorOffsetFromDB(sensor_id);
            if (locationOffset.ContainsKey(sensor_id))
            {
                bearing.Az += locationOffset[sensor_id]["az_offset"];
                bearing.Ele += locationOffset[sensor_id]["ele_offset"];
            }

            return bearing;
        }
    }
}
