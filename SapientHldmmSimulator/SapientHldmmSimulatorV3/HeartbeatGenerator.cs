// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: HeartbeatGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using System;
    using System.Threading;
    using log4net;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate SAPIENT heartbeat (status report messages)
    /// </summary>
    public class HeartbeatGenerator
    {
        public int ASMId { get; set; }

        public int HeartbeatTime { get; set; }

        public bool LoopHeatbeat { get; set; }

        public int Azimuth { get; set; }

        private int heartbeat_id = 1;

        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor
        /// </summary>
        public HeartbeatGenerator()
        {

        }

        /// <summary>
        /// sends out health messages, will loop until manually told not too
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        public void GenerateHLStatus(object comms_connection)
        {
            do
            {
                const double sensorX = -2.313;
                const double sensorY = 52.101;
                const double obsX = 0.0001;
                const double obsY = 0.0001;
                const double fieldOfViewX = 0.001;

                var messenger = (IConnection)comms_connection;

                locationList sensorStatusRegion1 = new locationList
                {
                    location = new[]
                            {
                        new location {X = sensorX + fieldOfViewX - obsX, Y = sensorY},
                        new location {X = sensorX + fieldOfViewX, Y = sensorY + obsY},
                        new location {X = sensorX + fieldOfViewX, Y = sensorY},
                        new location {X = sensorX + fieldOfViewX - obsX, Y = sensorY}
                    }
                };

                locationList sensorStatusRegion2 = new locationList
                {
                    location = new[]
                            {
                        new location {X = sensorX + fieldOfViewX - obsX, Y = sensorY},
                        new location {X = sensorX + fieldOfViewX, Y = sensorY - obsY},
                        new location {X = sensorX + fieldOfViewX, Y = sensorY},
                        new location {X = sensorX + fieldOfViewX - obsX, Y = sensorY}
                    }
                };

                StatusReportStatusRegion statusRegion1 = new StatusReportStatusRegion
                {
                    type = "Area Of Interest",
                    regionID = 1,
                    regionName = "region 1",
                    regionStatus = "Unchanged",
                    description = "Description:a region in area 1",
                    locationList = sensorStatusRegion1
                };

                StatusReportStatusRegion statusRegion2 = new StatusReportStatusRegion
                {
                    type = "Area Of Interest",
                    regionID = 2,
                    regionName = "region 2",
                    regionStatus = "Unchanged",
                    description = "Description:a region in area 2",
                    locationList = sensorStatusRegion2
                };

                StatusReport status = new StatusReport
                {
                    timestamp = DateTime.UtcNow,
                    sourceID = ASMId,
                    reportID = heartbeat_id++,
                    system = "OK",
                    info = "Unchanged",
                    activeTaskID = 0,
                    statusRegion = new StatusReportStatusRegion[] { statusRegion1, statusRegion2 }
                };

                if (heartbeat_id > 2)
                {
                    status.sensorLocation = null;
                }

                var xmlstring = ConfigXMLParser.Serialize(status);
                var record_bytes = System.Text.Encoding.UTF8.GetBytes(xmlstring);
                if (messenger.SendMessage(record_bytes, record_bytes.Length))
                {
                    Log.InfoFormat("Sent StatusReport: {0}", status.reportID);
                }
                else
                {
                    Log.InfoFormat("Sent StatusReport Failed: {0}", status.reportID);
                }

                if (LoopHeatbeat) Thread.Sleep(HeartbeatTime);
            } while (LoopHeatbeat);
        }
    }
}
