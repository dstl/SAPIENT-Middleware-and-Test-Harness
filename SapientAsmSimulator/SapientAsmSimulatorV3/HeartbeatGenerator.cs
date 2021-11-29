// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: HeartbeatGenerator.cs$
// <copyright file="HeartbeatGenerator.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.Threading;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Heartbeat messages
    /// </summary>
    public class HeartbeatGenerator : XmlGenerators
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the HeartbeatGenerator class. 
        /// </summary>
        public HeartbeatGenerator()
        {
            Azimuth = 125;
            Elevation = 0;
            Range = 50;
            HExtent = 30;
        }

        /// <summary>
        /// Gets or sets the Azimuth angle to use for the sensor field of view
        /// </summary>
        public static double Azimuth { get; set; }

        /// <summary>
        /// Gets or sets the Elevation angle to use for the sensor field of view
        /// </summary>
        public static double Elevation { get; set; }

        /// <summary>
        /// Gets or sets the Range to use for the sensor field of view
        /// </summary>
        public static double Range { get; set; }

        /// <summary>
        /// Gets or sets the Horizontal Field of View to use for the sensor field of view
        /// </summary>
        public static double HExtent { get; set; }

        public static bool UseRBCoverage { get; set; }

        /// <summary>
        /// Populate status report with example values
        /// </summary>
        /// <returns>Populated StatusReport object</returns>
        public StatusReport GenerateStatusReportWithObsAndCoverage()
        {
            double sensorX = start_longitude;
            double sensorY = start_latitude;
            const double CoverageX = 0.001;
            const double CoverageY = 0.001;
            const double ObscurationX = 0.0001;
            const double ObscurationY = 0.0001;
            const double FieldOfViewX = 0.001;

            locationList sensorObs1 = new locationList
            {
                location = new[]
                {
                    new location { X = sensorX + FieldOfViewX - ObscurationX, Y = sensorY },
                    new location { X = sensorX + FieldOfViewX, Y = sensorY + ObscurationY },
                    new location { X = sensorX + FieldOfViewX, Y = sensorY },
                    new location { X = sensorX + FieldOfViewX - ObscurationX, Y = sensorY }
                }
            };

            locationList sensorObs2 = new locationList
            {
                location = new[]
                {
                    new location { X = sensorX + FieldOfViewX - ObscurationX, Y = sensorY },
                    new location { X = sensorX + FieldOfViewX, Y = sensorY - ObscurationY },
                    new location { X = sensorX + FieldOfViewX, Y = sensorY },
                    new location { X = sensorX + FieldOfViewX - ObscurationX, Y = sensorY }
                }
            };

            locationList sensorCoverage = new locationList
            {
                location = new[]
                {
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY }
                }
            };

            StatusReport status = new StatusReport
            {
                timestamp = DateTime.UtcNow,
                sourceID = ASMId,
                reportID = MessageCount,
                system = "OK",
                info = "New",
                activeTaskID = 0,
                mode = "Mode",
                power = new StatusReportPower { source = "Mains", status = "OK", level = 50 },
                sensorLocation = new StatusReportSensorLocation { location = new location { X = sensorX, Y = sensorY, Z = 57.0, ZSpecified = true } },
                status = new[] 
                {
                    new StatusReportStatus { level = "Error", type = "InternalFault", value = "Sensor Data Lost" },
                    new StatusReportStatus { level = "Sensor", type = "Exposure", value = "Low" },
                },
                obscuration = new[] { new StatusReportObscuration { locationList = sensorObs1 }, new StatusReportObscuration { locationList = sensorObs2 } },
                coverage = new StatusReportCoverage { locationList = sensorCoverage },
                fieldOfView = new StatusReportFieldOfView(),
            };

            status.fieldOfView.rangeBearingCone = this.GetFieldOfViewCone();

            Azimuth += 5;
            if (Azimuth > 200)
            {
                Azimuth = 125;
            }

            this.MessageCount++;

            return status;
        }

        /// <summary>
        /// sends out health messages, will loop until manually told not too
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="logger">file data logger</param>
        public void GenerateStatus(object comms_connection, SapientLogger logger)
        {
            IConnection messenger = (IConnection)comms_connection;

            do
            {
                long reportID = this.MessageCount;
                bool UseRBCoverage = true;

                StatusReport statusReport = this.GeneratePTZStatusReport(UseRBCoverage);

                string xmlstring = ConfigXMLParser.Serialize(statusReport);
                bool retval = MessageSender.Send(messenger, xmlstring, logger);

                if (retval)
                {
                    Log.InfoFormat("Sent StatusReport:{0}", reportID);
                }
                else
                {
                    Log.ErrorFormat("Sent StatusReport Failed:{0}", reportID);
                }

                if (this.LoopMessages)
                {
                    Thread.Sleep(this.LoopTime);
                }
            } while (this.LoopMessages);
        }

        /// <summary>
        /// Return a field of cone of the current range, azimuth and elevation
        /// </summary>
        /// <returns>populated rangeBearingCone object</returns>
        private rangeBearingCone GetFieldOfViewCone()
        {
            rangeBearingCone cone = new rangeBearingCone();
            cone.Az = Azimuth;
            cone.R = Range;
            cone.hExtent = HExtent;
            cone.Ele = Elevation;
            cone.EleSpecified = true;

            // if has been tasked without range then use default
            if(cone.R==0)
            {
                cone.R = 50;
            }
     
            return cone;
        }

        /// <summary>
        /// Populate status report with PTZ like values
        /// </summary>
        /// <returns>populated StatusReport object</returns>
        private StatusReport GeneratePTZStatusReport(bool useRB)
        {
            double sensorX = start_longitude;
            double sensorY = start_latitude;

            StatusReport status = new StatusReport
            {
                timestamp = DateTime.UtcNow,
                sourceID = ASMId,
                reportID = this.MessageCount,
                system = "OK",
                info = "New",
                activeTaskID = 0,
                mode = "Mode",
                power = new StatusReportPower { source = "Mains", status = "OK" },
                sensorLocation = new StatusReportSensorLocation { location = new location { X = sensorX, Y = sensorY, Z = 2.0, ZSpecified = true } },
                fieldOfView = new StatusReportFieldOfView(),
            };

            if (useRB)
            {
                status.fieldOfView.rangeBearingCone = this.GetFieldOfViewCone();
            }
            else
            {
                status.coverage = new StatusReportCoverage()
                {
                    locationList = GenerateCoverage()
                };
            }

            this.MessageCount++;

            return status;
        }

        private locationList GenerateCoverage()
        {
            double sensorX = start_longitude;
            double sensorY = start_latitude;
            const double CoverageX = 30;
            const double CoverageY = 30;

            locationList sensorCoverage = new locationList
            {
                location = new[]
                {
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY }
                }
            };

            return sensorCoverage;
        }
    }
}
